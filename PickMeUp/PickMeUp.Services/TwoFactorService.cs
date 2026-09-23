using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PickMeUp.Common.Services.Configuration;
using PickMeUp.Common.Services.CryptoService;
using PickMeUp.Model.Access;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Messaging;
using PickMeUp.Services.Database;
using PickMeUp.Services.Messaging;

namespace PickMeUp.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly PickMeUpDbContext _dbContext;
        private readonly ICryptoService _cryptoService;
        private readonly IEventPublisher _eventPublisher;
        private readonly TwoFactorOptions _options;
        private readonly ILogger<TwoFactorService> _logger;

        public TwoFactorService(
            PickMeUpDbContext dbContext,
            ICryptoService cryptoService,
            IEventPublisher eventPublisher,
            IOptions<TwoFactorOptions> options,
            ILogger<TwoFactorService> logger)
        {
            _dbContext = dbContext;
            _cryptoService = cryptoService;
            _eventPublisher = eventPublisher;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<TwoFactorChallenge> IssueChallengeAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await LoadUserAsync(userId);
            var now = DateTime.UtcNow;

            var live = await _dbContext.TwoFactorCodes
                .Where(c => c.UserId == userId && c.ConsumedAt == null && c.ExpiresAt > now)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (live != null && live.CreatedAt.AddSeconds(_options.ResendCooldownSeconds) > now)
            {
                return new TwoFactorChallenge(
                    MaskEmail(user.Email),
                    SecondsUntil(live.ExpiresAt, now),
                    CodeSent: false);
            }

            await InvalidateLiveCodesAsync(userId, now, cancellationToken);

            var code = GenerateCode();
            var salt = _cryptoService.GenerateSlat();
            var expiresAt = now.AddMinutes(_options.ExpiryMinutes);

            _dbContext.TwoFactorCodes.Add(new TwoFactorCode
            {
                UserId = userId,
                CodeHash = _cryptoService.GenerateHash(code, salt),
                CodeSalt = salt,
                CreatedAt = now,
                ExpiresAt = expiresAt
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            await PublishCodeAsync(user, code, cancellationToken);

            return new TwoFactorChallenge(
                MaskEmail(user.Email),
                SecondsUntil(expiresAt, now),
                CodeSent: true);
        }

        public async Task<int> VerifyAsync(int userId, string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new BusinessException("Enter the code we sent you.");
            }

            var now = DateTime.UtcNow;

            var challenge = await _dbContext.TwoFactorCodes
                .Where(c => c.UserId == userId && c.ConsumedAt == null && c.ExpiresAt > now)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (challenge == null)
            {
                throw new BusinessException("That code has expired. Sign in again to get a new one.");
            }

            challenge.AttemptCount++;

            if (challenge.AttemptCount > _options.MaxAttempts)
            {
                challenge.ConsumedAt = now;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Two-factor challenge for user {UserId} was blocked after too many attempts.", userId);

                throw new BusinessException("Too many wrong codes. Sign in again to get a new one.");
            }

            if (!_cryptoService.Verify(challenge.CodeHash, challenge.CodeSalt, code.Trim()))
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new BusinessException("That code is not correct.");
            }

            challenge.ConsumedAt = now;

            var user = await LoadUserAsync(userId);

            if (user.TwoFactorMode == TwoFactorModeEnum.ConfirmOnce && user.TwoFactorConfirmedAt == null)
            {
                user.TwoFactorConfirmedAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return userId;
        }

        public async Task<TwoFactorSettingResponse> GetSettingAsync(int userId)
        {
            var user = await LoadUserAsync(userId);

            return new TwoFactorSettingResponse
            {
                Enabled = user.TwoFactorMode != TwoFactorModeEnum.Off,
                MaskedEmail = MaskEmail(user.Email)
            };
        }

        public async Task<TwoFactorSettingResponse> SetEnabledAsync(int userId, bool enabled)
        {
            var user = await LoadUserAsync(userId);

            if (enabled && string.IsNullOrWhiteSpace(user.Email))
            {
                throw new BusinessException("Add an email address to your account before you turn this on.");
            }

            user.TwoFactorMode = enabled ? TwoFactorModeEnum.EveryLogin : TwoFactorModeEnum.Off;
            user.TwoFactorConfirmedAt = null;

            if (!enabled)
            {
                await InvalidateLiveCodesAsync(userId, DateTime.UtcNow);
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Two-factor authentication was {State} for user {UserId}.",
                enabled ? "enabled" : "disabled", userId);

            return new TwoFactorSettingResponse
            {
                Enabled = enabled,
                MaskedEmail = MaskEmail(user.Email)
            };
        }

        public string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return string.Empty;
            }

            var at = email.IndexOf('@');

            if (at <= 0)
            {
                return "***";
            }

            return $"{email[0]}***{email[at..]}";
        }

        private async Task<User> LoadUserAsync(int userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new BusinessException("User not found.");
            }

            return user;
        }

        private async Task InvalidateLiveCodesAsync(int userId, DateTime now, CancellationToken cancellationToken = default)
        {
            await _dbContext.TwoFactorCodes
                .Where(c => c.UserId == userId && c.ConsumedAt == null)
                .ExecuteUpdateAsync(c => c.SetProperty(x => x.ConsumedAt, now), cancellationToken);
        }

        private async Task PublishCodeAsync(User user, string code, CancellationToken cancellationToken)
        {
            try
            {
                await _eventPublisher.PublishAsync(new TwoFactorCodeIssuedEvent
                {
                    RecipientEmail = user.Email,
                    RecipientName = $"{user.FirstName} {user.LastName}".Trim(),
                    Code = code,
                    ExpiresInMinutes = _options.ExpiryMinutes
                }, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not queue the sign-in code for user {UserId}.", user.Id);
                throw new BusinessException("We could not send your sign-in code right now. Please try again.");
            }
        }

        private string GenerateCode()
        {
            var digits = new char[_options.CodeLength];

            for (var i = 0; i < digits.Length; i++)
            {
                digits[i] = (char)('0' + RandomNumberGenerator.GetInt32(10));
            }

            return new string(digits);
        }

        private static int SecondsUntil(DateTime moment, DateTime now)
        {
            var remaining = (int)Math.Ceiling((moment - now).TotalSeconds);
            return remaining < 0 ? 0 : remaining;
        }
    }
}
