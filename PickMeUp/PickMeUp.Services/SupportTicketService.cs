using PickMeUp.Model.Constants;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services
{
    public class SupportTicketService
        : BaseReadService<SupportTicket, SupportTicketResponse, SupportTicketSearch>, ISupportTicketService
    {
        private readonly IAuthenticatedUserAccessor _userAccessor;
        private readonly IValidator<SupportTicketInsertRequest> _insertValidator;
        private readonly IValidator<SupportTicketReplyRequest> _replyValidator;

        private int? _scopedSubmitterId;
        private bool _scopeResolved;

        public SupportTicketService(
            MapsterMapper.IMapper mapper,
            PickMeUpDbContext dbContext,
            IAuthenticatedUserAccessor userAccessor,
            IValidator<SupportTicketInsertRequest> insertValidator,
            IValidator<SupportTicketReplyRequest> replyValidator)
            : base(mapper, dbContext)
        {
            _userAccessor = userAccessor;
            _insertValidator = insertValidator;
            _replyValidator = replyValidator;
        }

        public async Task<SupportTicketResponse> SubmitAsync(SupportTicketInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            var userId = RequireUserId();

            if (request.TargetUserId.HasValue)
            {
                var exists = await _dbContext.Users.AnyAsync(u => u.Id == request.TargetUserId.Value);

                if (!exists)
                {
                    throw new BusinessException("The account this report is about could not be found.");
                }
            }

            var ticket = new SupportTicket
            {
                SubmitterId = userId,
                Subject = request.Subject,
                Body = request.Body,
                TargetUserId = request.TargetUserId,
                SupportTicketStatusId = (int)SupportTicketStatusEnum.Open,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.SupportTickets.Add(ticket);
            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(ticket.Id);
        }

        public async Task<SupportTicketResponse> ReplyAsync(int id, SupportTicketReplyRequest request)
        {
            await _replyValidator.ValidateAndThrowAsync(request);

            var userId = RequireUserId();
            var ticket = await LoadTicketAsync(id);

            _dbContext.SupportTicketReplies.Add(new SupportTicketReply
            {
                SupportTicketId = ticket.Id,
                AuthorId = userId,
                Body = request.Body,
                CreatedAt = DateTime.UtcNow
            });

            if (ticket.SupportTicketStatusId == (int)SupportTicketStatusEnum.Open)
            {
                ticket.SupportTicketStatusId = (int)SupportTicketStatusEnum.InProgress;
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(ticket.Id);
        }

        public async Task<SupportTicketResponse> ChangeStatusAsync(int id, SupportTicketStatusChangeRequest request)
        {
            var userId = RequireUserId();
            var ticket = await LoadTicketAsync(id);

            if (ticket.SupportTicketStatusId == (int)request.Status)
            {
                throw new BusinessException($"This ticket is already {request.Status}.");
            }

            ticket.SupportTicketStatusId = (int)request.Status;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (request.Status == SupportTicketStatusEnum.Resolved)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
                ticket.ResolvedByUserId = userId;
            }
            else
            {
                ticket.ResolvedAt = null;
                ticket.ResolvedByUserId = null;
            }

            await _dbContext.SaveChangesAsync();

            return await LoadResponseAsync(ticket.Id);
        }

        protected override Task<IQueryable<SupportTicket>> IncludeRelatedEntitiesAsync(
            SupportTicketSearch? search, IQueryable<SupportTicket> query)
        {
            IQueryable<SupportTicket> result = query
                .Include(t => t.Submitter)
                .Include(t => t.SupportTicketStatus)
                .Include(t => t.TargetUser)
                .Include(t => t.ResolvedByUser)
                .Include(t => t.Replies).ThenInclude(r => r.Author);

            return Task.FromResult(result);
        }

        protected override IQueryable<SupportTicket> ApplyFilters(
            IQueryable<SupportTicket> query, SupportTicketSearch? search)
        {
            if (_scopedSubmitterId.HasValue)
            {
                query = query.Where(t => t.SubmitterId == _scopedSubmitterId.Value);
            }

            if (search == null)
            {
                return query;
            }

            if (search.Status.HasValue)
            {
                query = query.Where(t => t.SupportTicketStatusId == (int)search.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.Subject))
            {
                var pattern = $"%{EscapeLikePattern(search.Subject)}%";
                query = query.Where(t => EF.Functions.ILike(t.Subject, pattern));
            }

            if (!string.IsNullOrWhiteSpace(search.SubmitterName))
            {
                var pattern = $"%{EscapeLikePattern(search.SubmitterName)}%";
                query = query.Where(t => EF.Functions.ILike(t.Submitter.FirstName, pattern)
                                      || EF.Functions.ILike(t.Submitter.LastName, pattern));
            }

            if (search.TargetUserId.HasValue)
            {
                query = query.Where(t => t.TargetUserId == search.TargetUserId.Value);
            }

            var createdFrom = search.CreatedFrom ?? PeriodStart(search.Period);
            if (createdFrom.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= createdFrom.Value);
            }

            if (search.CreatedTo.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= search.CreatedTo.Value);
            }

            return query;
        }

        public override async Task<PageResult<SupportTicketResponse>> GetAllAsync(SupportTicketSearch? search = null)
        {
            await ResolveScopeAsync();
            return await base.GetAllAsync(search);
        }

        public override async Task<SupportTicketResponse> GetByIdAsync(int id)
        {
            await ResolveScopeAsync();

            var ticket = await FindByIdAsync(id)
                ?? throw new KeyNotFoundException($"Support ticket with id {id} not found.");

            if (_scopedSubmitterId.HasValue && ticket.SubmitterId != _scopedSubmitterId.Value)
            {
                throw new KeyNotFoundException($"Support ticket with id {id} not found.");
            }

            return _mapper.Map<SupportTicketResponse>(ticket);
        }

        private async Task ResolveScopeAsync()
        {
            if (_scopeResolved)
            {
                return;
            }

            _scopedSubmitterId = _userAccessor.IsInRole(Roles.Admin)
                ? null
                : _userAccessor.GetUserId() ?? -1;

            _scopeResolved = true;
            await Task.CompletedTask;
        }

        private async Task<SupportTicket> LoadTicketAsync(int id) =>
            await _dbContext.SupportTickets.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Support ticket with id {id} not found.");

        private static DateTime? PeriodStart(ShiftPeriod? period) => period switch
        {
            ShiftPeriod.Day => DateTime.UtcNow.AddDays(-1),
            ShiftPeriod.Week => DateTime.UtcNow.AddDays(-7),
            ShiftPeriod.Month => DateTime.UtcNow.AddMonths(-1),
            _ => null
        };

        private int RequireUserId() =>
            _userAccessor.GetUserId() ?? throw new BusinessException("The caller could not be identified.");

        private async Task<SupportTicketResponse> LoadResponseAsync(int id)
        {
            var ticket = await _dbContext.SupportTickets
                .AsNoTracking()
                .Include(t => t.Submitter)
                .Include(t => t.SupportTicketStatus)
                .Include(t => t.TargetUser)
                .Include(t => t.ResolvedByUser)
                .Include(t => t.Replies).ThenInclude(r => r.Author)
                .FirstAsync(t => t.Id == id);

            return _mapper.Map<SupportTicketResponse>(ticket);
        }
    }
}
