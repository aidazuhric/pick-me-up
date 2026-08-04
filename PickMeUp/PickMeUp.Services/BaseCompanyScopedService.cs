using PickMeUp.Model.Constants;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services
{
    public abstract class BaseCompanyScopedService<TEntity, TResponse, TSearch, TInsertRequest, TUpdateRequest>
        : BaseCRUDService<TEntity, TResponse, TSearch, TInsertRequest, TUpdateRequest>
        where TEntity : class
        where TSearch : BaseSearchObject
    {
        protected readonly IAuthenticatedUserAccessor _userAccessor;

        private int? _scopedCompanyId;
        private bool _scopeResolved;

        protected BaseCompanyScopedService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<TInsertRequest> insertValidator,
            IValidator<TUpdateRequest> updateValidator,
            IAuthenticatedUserAccessor userAccessor)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _userAccessor = userAccessor;
        }

        protected abstract IQueryable<TEntity> ApplyCompanyScope(IQueryable<TEntity> query, int companyId);

        protected abstract int GetOwningCompanyId(TEntity entity);

        protected abstract IQueryable<TEntity> ApplyEntityFilters(IQueryable<TEntity> query, TSearch? search);

        protected bool CallerIsAdmin => _userAccessor.IsInRole(Roles.Admin);

        protected sealed override IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, TSearch? search)
        {
            if (_scopedCompanyId.HasValue)
            {
                query = ApplyCompanyScope(query, _scopedCompanyId.Value);
            }

            return ApplyEntityFilters(query, search);
        }

        public override async Task<PageResult<TResponse>> GetAllAsync(TSearch? search = null)
        {
            await ResolveReadScopeAsync();
            return await base.GetAllAsync(search);
        }

        public override async Task<TResponse> GetByIdAsync(int id)
        {
            var entity = await FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");

            await EnsureCallerOwnsAsync(entity, id);

            return _mapper.Map<TResponse>(entity);
        }

        public override async Task<TResponse> UpdateAsync(int id, TUpdateRequest request)
        {
            var entity = await FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");

            await EnsureCallerOwnsAsync(entity, id);

            return await base.UpdateAsync(id, request);
        }

        public override async Task DeleteAsync(int id)
        {
            var entity = await FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");

            await EnsureCallerOwnsAsync(entity, id);

            await base.DeleteAsync(id);
        }

        protected async Task<int> RequireCallerCompanyIdAsync()
        {
            var companyId = await GetCallerCompanyIdAsync();

            if (companyId == null)
            {
                throw new BusinessException("This account is not linked to a taxi company.");
            }

            return companyId.Value;
        }

        protected async Task<int?> GetCallerCompanyIdAsync()
        {
            var userId = _userAccessor.GetUserId();

            if (userId == null)
            {
                return null;
            }

            var ownedCompanyId = await _dbContext.TaxiCompanies
                .Where(t => t.UserId == userId.Value)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();

            if (ownedCompanyId != null)
            {
                return ownedCompanyId;
            }

            return await _dbContext.Drivers
                .Where(d => d.UserId == userId.Value && d.ResignedAt == null)
                .Select(d => (int?)d.TaxiCompanyId)
                .FirstOrDefaultAsync();
        }

        private async Task EnsureCallerOwnsAsync(TEntity entity, int id)
        {
            if (CallerIsAdmin)
            {
                return;
            }

            var companyId = await GetCallerCompanyIdAsync();

            if (companyId == null || GetOwningCompanyId(entity) != companyId.Value)
            {
                throw new KeyNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");
            }
        }

        private async Task ResolveReadScopeAsync()
        {
            if (_scopeResolved)
            {
                return;
            }

            _scopedCompanyId = CallerIsAdmin ? null : await GetCallerCompanyIdAsync() ?? -1;
            _scopeResolved = true;
        }
    }
}
