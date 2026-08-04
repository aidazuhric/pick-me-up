using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace PickMeUp.Services
{
    public class VehicleCategoryService
        : BaseReferenceService<VehicleCategory, VehicleCategoryResponse, VehicleCategorySearch, VehicleCategoryInsertRequest, VehicleCategoryUpdateRequest>, IVehicleCategoryService
    {
        public VehicleCategoryService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<VehicleCategoryInsertRequest> insertValidator,
            IValidator<VehicleCategoryUpdateRequest> updateValidator,
            IMemoryCache cache)
            : base(dbContext, mapper, insertValidator, updateValidator, cache)
        {
        }

        protected override string EntityDisplayName => "vehicle category";

        protected override IQueryable<VehicleCategory> ApplyFilters(IQueryable<VehicleCategory> query, VehicleCategorySearch? search)
        {
            if (search == null)
            {
                return query;
            }

            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                var pattern = $"%{EscapeLikePattern(search.Name)}%";
                query = query.Where(x => EF.Functions.ILike(x.Name, pattern));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == search.IsActive.Value);
            }

            return query;
        }
    }
}
