using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using System.Text;

namespace PickMeUp.Services
{
    public abstract class BaseReferenceService<TEntity, TResponse, TSearch, TInsertRequest, TUpdateRequest>
        : BaseCRUDService<TEntity, TResponse, TSearch, TInsertRequest, TUpdateRequest>
        where TEntity : class
        where TSearch : ReferenceSearchObject
    {
        private static readonly ConcurrentDictionary<Type, CancellationTokenSource> _invalidationTokens = new();
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(30);

        private readonly IMemoryCache _cache;

        protected BaseReferenceService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<TInsertRequest> insertValidator,
            IValidator<TUpdateRequest> updateValidator,
            IMemoryCache cache)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _cache = cache;
        }

        protected abstract string EntityDisplayName { get; }

        public override async Task<PageResult<TResponse>> GetAllAsync(TSearch? search = null)
        {
            var cacheKey = BuildCacheKey(search);

            if (_cache.TryGetValue(cacheKey, out PageResult<TResponse>? cached) && cached != null)
            {
                return cached;
            }

            var result = await base.GetAllAsync(search);

            _cache.Set(cacheKey, result, BuildCacheEntryOptions());

            return result;
        }

        public override async Task<TResponse> InsertAsync(TInsertRequest request)
        {
            var result = await base.InsertAsync(request);
            InvalidateCache();
            return result;
        }

        public override async Task<TResponse> UpdateAsync(int id, TUpdateRequest request)
        {
            var result = await base.UpdateAsync(id, request);
            InvalidateCache();
            return result;
        }

        public override async Task DeleteAsync(int id)
        {
            try
            {
                await base.DeleteAsync(id);
            }
            catch (DbUpdateException exception) when (exception.IsForeignKeyViolation())
            {
                throw new BusinessException(
                    $"This {EntityDisplayName} is used by existing records and cannot be deleted. Deactivate it instead.");
            }

            InvalidateCache();
        }

        private static string BuildCacheKey(TSearch? search)
        {
            var key = new StringBuilder(typeof(TEntity).FullName);

            key.Append("|name=").Append(search?.Name)
               .Append("|active=").Append(search?.IsActive)
               .Append("|page=").Append(search?.Page)
               .Append("|size=").Append(search?.PageSize)
               .Append("|sort=").Append(search?.SortBy)
               .Append("|count=").Append(search?.IncludeTotalCount);

            return key.ToString();
        }

        private static MemoryCacheEntryOptions BuildCacheEntryOptions()
        {
            var tokenSource = _invalidationTokens.GetOrAdd(typeof(TEntity), _ => new CancellationTokenSource());

            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheLifetime)
                .AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
        }

        private static void InvalidateCache()
        {
            if (_invalidationTokens.TryRemove(typeof(TEntity), out var tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }
    }
}
