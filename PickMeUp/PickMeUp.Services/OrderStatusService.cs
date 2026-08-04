using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace PickMeUp.Services
{
    public class OrderStatusService
        : BaseReferenceService<OrderStatus, OrderStatusResponse, OrderStatusSearch, OrderStatusInsertRequest, OrderStatusUpdateRequest>, IOrderStatusService
    {
        public OrderStatusService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<OrderStatusInsertRequest> insertValidator,
            IValidator<OrderStatusUpdateRequest> updateValidator,
            IMemoryCache cache)
            : base(dbContext, mapper, insertValidator, updateValidator, cache)
        {
        }

        protected override string EntityDisplayName => "order status";

        protected override IQueryable<OrderStatus> ApplyFilters(IQueryable<OrderStatus> query, OrderStatusSearch? search)
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
