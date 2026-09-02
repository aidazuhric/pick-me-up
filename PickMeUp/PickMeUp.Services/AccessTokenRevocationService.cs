using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PickMeUp.Common.Services.Configuration;
using PickMeUp.Services.Database;

namespace PickMeUp.Services
{
    public class AccessTokenRevocationService : IAccessTokenRevocationService
    {
        private const string CachePrefix = "revoked-access-token:";
        private static DateTime _lastSweep = DateTime.MinValue;
        private static readonly SemaphoreSlim SweepGate = new(1, 1);

        private readonly PickMeUpDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly AccessTokenRevocationOptions _options;

        public AccessTokenRevocationService(
            PickMeUpDbContext dbContext,
            IMemoryCache cache,
            IOptions<AccessTokenRevocationOptions> options)
        {
            _dbContext = dbContext;
            _cache = cache;
            _options = options.Value;
        }

        public async Task RevokeAsync(
            string tokenId, int userId, DateTime expiresAt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return;
            }

            var alreadyRevoked = await _dbContext.RevokedAccessTokens
                .AnyAsync(t => t.TokenId == tokenId, cancellationToken);

            if (!alreadyRevoked)
            {
                _dbContext.RevokedAccessTokens.Add(new RevokedAccessToken
                {
                    TokenId = tokenId,
                    UserId = userId,
                    ExpiresAt = expiresAt,
                    RevokedAt = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            Remember(tokenId, expiresAt);

            await SweepExpiredAsync(cancellationToken);
        }

        public async Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return false;
            }

            if (_cache.TryGetValue(CachePrefix + tokenId, out _))
            {
                return true;
            }

            var revoked = await _dbContext.RevokedAccessTokens
                .AsNoTracking()
                .Where(t => t.TokenId == tokenId)
                .Select(t => (DateTime?)t.ExpiresAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (revoked == null)
            {
                return false;
            }

            Remember(tokenId, revoked.Value);

            return true;
        }

        private void Remember(string tokenId, DateTime expiresAt)
        {
            var remaining = expiresAt - DateTime.UtcNow;

            if (remaining <= TimeSpan.Zero)
            {
                return;
            }

            _cache.Set(CachePrefix + tokenId, true, remaining);
        }

        private async Task SweepExpiredAsync(CancellationToken cancellationToken)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.SweepMinutes));

            if (DateTime.UtcNow - _lastSweep < interval)
            {
                return;
            }

            if (!await SweepGate.WaitAsync(0, cancellationToken))
            {
                return;
            }

            try
            {
                if (DateTime.UtcNow - _lastSweep < interval)
                {
                    return;
                }

                await _dbContext.RevokedAccessTokens
                    .Where(t => t.ExpiresAt < DateTime.UtcNow)
                    .ExecuteDeleteAsync(cancellationToken);

                _lastSweep = DateTime.UtcNow;
            }
            finally
            {
                SweepGate.Release();
            }
        }
    }
}
