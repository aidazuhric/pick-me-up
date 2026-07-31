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

        /// <summary>
        /// Sortable property names per entity type, resolved once by reflection.
        /// Only scalar properties are sortable — navigation properties and collections are excluded.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, HashSet<string>> _sortableProperties = new();

        protected BaseReadService(MapsterMapper.IMapper mapper, PickMeUpDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Applies search filters to the query. Override in derived classes to implement specific filtering logic.
        /// Keep expressions EF-translatable — this runs in the database, not in memory.
        /// </summary>
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

        /// <summary>
        /// Applies <paramref name="sortBy"/> after validating it against the entity's sortable properties.
        /// Accepts "Property" or "Property asc" / "Property desc". Anything else is rejected rather than
        /// passed through, because Dynamic LINQ would otherwise interpret arbitrary caller input.
        /// Falls back to ordering by Id (when present) so paging is deterministic.
        /// </summary>
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
                throw new ClinetException($"Invalid sort expression '{sortBy}'.");
            }

            if (!sortable.TryGetValue(parts[0], out var canonicalName))
            {
                throw new ClinetException($"Cannot sort by '{parts[0]}'.");
            }

            var direction = "asc";
            if (parts.Length == 2)
            {
                direction = parts[1].ToLowerInvariant();
                if (direction is not ("asc" or "desc"))
                {
                    throw new ClinetException($"Invalid sort direction '{parts[1]}'. Use 'asc' or 'desc'.");
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

        /// <summary>
        /// Escapes ILIKE wildcards so user input is matched literally.
        /// Use when building <c>EF.Functions.ILike</c> patterns in <see cref="ApplyFilters"/>.
        /// </summary>
        protected static string EscapeLikePattern(string value) =>
            value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
    }
}
