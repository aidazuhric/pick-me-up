using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace PickMeUp.Services
{
    public abstract class BaseReadService<TEntity, TResponse, TSearch> : IBaseReadService<TResponse, TSearch>
        where TEntity : class
        where TSearch : BaseSearchObject
    {
        protected readonly MapsterMapper.IMapper _mapper;
        protected readonly PickMeUpDbContext _dbContext;

        private static readonly ConcurrentDictionary<Type, HashSet<string>> _sortableProperties = new();

        protected BaseReadService(MapsterMapper.IMapper mapper, PickMeUpDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        protected abstract IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, TSearch? search);

        public virtual async Task<PageResult<TResponse>> GetAllAsync(TSearch? search = null)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            query = await IncludeRelatedEntitiesAsync(search, query);
            query = ApplyFilters(query, search);

            int? totalCount = null;
            if (search?.IncludeTotalCount ?? false)
            {
                totalCount = await query.CountAsync();
            }

            query = ApplySorting(query, search?.SortBy);

            var page = search?.Page;
            var pageSize = search?.PageSize;

            if (page.HasValue && pageSize.HasValue)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value);
            }

            if (pageSize.HasValue)
            {
                query = query.Take(pageSize.Value);
            }

            var entities = await query.ToListAsync();
            var items = entities.Select(entity => _mapper.Map<TResponse>(entity)).ToList();

            return new PageResult<TResponse>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        private static IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string? sortBy)
        {
            var sortable = _sortableProperties.GetOrAdd(typeof(TEntity), static type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && IsSortableType(p.PropertyType))
                    .Select(p => p.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return sortable.Contains("Id") ? query.OrderBy("Id") : query;
            }

            var parts = sortBy.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 2)
            {
                throw new BusinessException($"Invalid sort expression '{sortBy}'.");
            }

            if (!sortable.TryGetValue(parts[0], out var canonicalName))
            {
                throw new BusinessException($"Cannot sort by '{parts[0]}'.");
            }

            var direction = "asc";
            if (parts.Length == 2)
            {
                direction = parts[1].ToLowerInvariant();
                if (direction is not ("asc" or "desc"))
                {
                    throw new BusinessException($"Invalid sort direction '{parts[1]}'. Use 'asc' or 'desc'.");
                }
            }

            return query.OrderBy($"{canonicalName} {direction}");
        }

        private static bool IsSortableType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying.IsPrimitive
                || underlying.IsEnum
                || underlying == typeof(string)
                || underlying == typeof(decimal)
                || underlying == typeof(DateTime)
                || underlying == typeof(DateTimeOffset)
                || underlying == typeof(TimeSpan)
                || underlying == typeof(Guid);
        }

        protected virtual Task<IQueryable<TEntity>> IncludeRelatedEntitiesAsync(TSearch? search, IQueryable<TEntity> query)
        {
            return Task.FromResult(query);
        }

        public virtual async Task<TResponse> GetByIdAsync(int id)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");
            }

            return _mapper.Map<TResponse>(entity);
        }

        protected static string EscapeLikePattern(string value) =>
            value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
    }
}
