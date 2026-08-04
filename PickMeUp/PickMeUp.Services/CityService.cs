using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace PickMeUp.Services
{
    public class CityService
        : BaseReferenceService<City, CityResponse, CitySearch, CityInsertRequest, CityUpdateRequest>, ICityService
    {
        public CityService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<CityInsertRequest> insertValidator,
            IValidator<CityUpdateRequest> updateValidator,
            IMemoryCache cache)
            : base(dbContext, mapper, insertValidator, updateValidator, cache)
        {
        }

        protected override string EntityDisplayName => "city";

        protected override IQueryable<City> ApplyFilters(IQueryable<City> query, CitySearch? search)
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
