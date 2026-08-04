using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace PickMeUp.Services
{
    public class PaymentStatusService
        : BaseReferenceService<PaymentStatus, PaymentStatusResponse, PaymentStatusSearch, PaymentStatusInsertRequest, PaymentStatusUpdateRequest>, IPaymentStatusService
    {
        public PaymentStatusService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<PaymentStatusInsertRequest> insertValidator,
            IValidator<PaymentStatusUpdateRequest> updateValidator,
            IMemoryCache cache)
            : base(dbContext, mapper, insertValidator, updateValidator, cache)
        {
        }

        protected override string EntityDisplayName => "payment status";

        protected override IQueryable<PaymentStatus> ApplyFilters(IQueryable<PaymentStatus> query, PaymentStatusSearch? search)
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
