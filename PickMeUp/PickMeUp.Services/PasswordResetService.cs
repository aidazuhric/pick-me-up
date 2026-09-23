using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PickMeUp.Common.Services.Configuration;
using PickMeUp.Common.Services.CryptoService;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Messaging;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Services.Database;
using PickMeUp.Services.Messaging;

namespace PickMeUp.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private const int MinPasswordLength = 6;
        private const int MaxPasswordLength = 100;
        private const string NeutralMessage =
            "If that email address belongs to an account, we have sent it a reset code.";

        private static readonly char[] TokenAlphabet =
            "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

        private readonly PickMeUpDbContext _dbContext;
        private readonly ICryptoService _cryptoService;
        private readonly IEventPublisher _eventPublisher;
        private readonly PasswordResetOptions _options;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            PickMeUpDbContext dbContext,
            ICryptoService cryptoService,
            IEventPublisher eventPublisher,
            IOptions<PasswordResetOptions> options,
            ILogger<PasswordResetService> logger)
        {
            _dbContext = dbContext;
            _cryptoService = cryptoService;
            _eventPublisher = eventPublisher;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<PasswordResetResponse> RequestAsync(
            ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var response = new PasswordResetResponse
            {
                Message = NeutralMessage,
                ExpiresInMinutes = _options.ExpiryMinutes
            };

            var email = (request.Email ?? string.Empty).Trim();

            if (email.Length == 0)
            {
                return response;
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);

            if (user == null)
            {
                _logger.LogInformation("Password reset asked for an address with no active account.");
                return response;
            }

            var now = DateTime.UtcNow;

            await InvalidateLiveTokensAsync(user.Id, now, cancellationToken);

            var token = GenerateToken();
            var salt = _cryptoService.GenerateSlat();

            _dbContext.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = _cryptoService.GenerateHash(token, salt),
                TokenSalt = salt,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(_options.ExpiryMinutes)
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            await PublishTokenAsync(user, token, cancellationToken);

            return response;
        }

        public async Task ResetAsync(
            ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var newPassword = request.NewPassword ?? string.Empty;

            if (newPassword.Length < MinPasswordLength)
            {
                throw new BusinessException(
                    $"New password must be at least {MinPasswordLength} characters.");
            }

            if (newPassword.Length > MaxPasswordLength)
            {
                throw new BusinessException(
                    $"New password cannot exceed {MaxPasswordLength} characters.");
            }

            if (!newPassword.Equals(request.ConfirmNewPassword))
            {
                throw new BusinessException("The two new passwords do not match.");
            }

            var email = (request.Email ?? string.Empty).Trim();
            var token = (request.Token ?? string.Empty).Trim();

            if (email.Length == 0 || token.Length == 0)
            {
                throw new BusinessException("That reset code is not valid. Ask for a new one.");
            }

            var now = DateTime.UtcNow;

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);

            if (user == null)
            {
                throw new BusinessException("That reset code is not valid. Ask for a new one.");
            }

            var stored = await _dbContext.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.ConsumedAt == null && t.ExpiresAt > now)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (stored == null)
            {
                throw new BusinessException(
                    "That reset code has expired or has already been used. Ask for a new one.");
            }

            stored.AttemptCount++;

            if (stored.AttemptCount > _options.MaxAttempts)
            {
                stored.ConsumedAt = now;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Password reset for user {UserId} was blocked after too many wrong codes.", user.Id);

                throw new BusinessException("Too many wrong codes. Ask for a new reset code.");
            }

            if (!_cryptoService.Verify(stored.TokenHash, stored.TokenSalt, token))
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new BusinessException("That reset code is not correct.");
            }

            var salt = _cryptoService.GenerateSlat();

            user.PasswordSalt = salt;
            user.PasswordHash = _cryptoService.GenerateHash(newPassword, salt);
            user.MustChangePassword = false;

            stored.ConsumedAt = now;

            await _dbContext.RefreshTokens
                .Where(t => t.UserId == user.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password reset completed for user {UserId}.", user.Id);
        }

        private async Task InvalidateLiveTokensAsync(
            int userId, DateTime now, CancellationToken cancellationToken)
        {
            await _dbContext.PasswordResetTokens
                .Where(t => t.UserId == userId && t.ConsumedAt == null)
                .ExecuteUpdateAsync(t => t.SetProperty(x => x.ConsumedAt, now), cancellationToken);
        }

        private string GenerateToken()
        {
            var length = Math.Clamp(_options.TokenLength, 6, 32);
            var buffer = new char[length];

            for (var i = 0; i < length; i++)
            {
                buffer[i] = TokenAlphabet[RandomNumberGenerator.GetInt32(TokenAlphabet.Length)];
            }

            return new string(buffer);
        }

        private async Task PublishTokenAsync(User user, string token, CancellationToken cancellationToken)
        {
            try
            {
                await _eventPublisher.PublishAsync(new PasswordResetRequestedEvent
                {
                    Token = token,
                    ExpiresInMinutes = _options.ExpiryMinutes,
                    RecipientEmail = user.Email,
                    RecipientName = $"{user.FirstName} {user.LastName}".Trim()
                }, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Could not queue the reset email for user {UserId}. The code is stored and still valid.",
                    user.Id);
            }
        }
    }
}
