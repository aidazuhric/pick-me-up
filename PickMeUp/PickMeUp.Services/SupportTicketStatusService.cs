using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace PickMeUp.Services
{
    public class SupportTicketStatusService
        : BaseReferenceService<SupportTicketStatus, SupportTicketStatusResponse, SupportTicketStatusSearch, SupportTicketStatusInsertRequest, SupportTicketStatusUpdateRequest>, ISupportTicketStatusService
    {
        public SupportTicketStatusService(
            PickMeUpDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<SupportTicketStatusInsertRequest> insertValidator,
            IValidator<SupportTicketStatusUpdateRequest> updateValidator,
            IMemoryCache cache)
            : base(dbContext, mapper, insertValidator, updateValidator, cache)
        {
        }

        protected override string EntityDisplayName => "support ticket status";

        protected override IQueryable<SupportTicketStatus> ApplyFilters(IQueryable<SupportTicketStatus> query, SupportTicketStatusSearch? search)
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
