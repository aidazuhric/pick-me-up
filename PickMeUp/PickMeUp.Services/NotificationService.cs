using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services
{
    public class NotificationService
        : BaseReadService<Notification, NotificationResponse, NotificationSearch>, INotificationService
    {
        private const string NewestFirst = "CreatedAt desc";

        private readonly IAuthenticatedUserAccessor _userAccessor;

        public NotificationService(
            MapsterMapper.IMapper mapper,
            PickMeUpDbContext dbContext,
            IAuthenticatedUserAccessor userAccessor)
            : base(mapper, dbContext)
        {
            _userAccessor = userAccessor;
        }

        public override Task<PageResult<NotificationResponse>> GetAllAsync(NotificationSearch? search = null)
        {
            search ??= new NotificationSearch();

            if (string.IsNullOrWhiteSpace(search.SortBy))
            {
                search.SortBy = NewestFirst;
            }

            return base.GetAllAsync(search);
        }

        public override async Task<NotificationResponse> GetByIdAsync(int id)
        {
            return _mapper.Map<NotificationResponse>(await LoadOwnAsync(id));
        }

        public async Task<UnreadCountResponse> GetUnreadCountAsync()
        {
            var userId = RequireUserId();

            return new UnreadCountResponse
            {
                UnreadCount = await _dbContext.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead)
            };
        }

        public async Task<NotificationResponse> MarkReadAsync(int id)
        {
            var notification = await LoadOwnAsync(id);

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return _mapper.Map<NotificationResponse>(notification);
        }

        public async Task<UnreadCountResponse> MarkAllReadAsync()
        {
            var userId = RequireUserId();
            var readAt = DateTime.UtcNow;

            await _dbContext.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, readAt));

            return await GetUnreadCountAsync();
        }

        protected override IQueryable<Notification> ApplyFilters(
            IQueryable<Notification> query, NotificationSearch? search)
        {
            var userId = RequireUserId();

            query = query.Where(n => n.UserId == userId);

            if (search == null)
            {
                return query;
            }

            if (search.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == search.IsRead.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.Category))
            {
                query = query.Where(n => n.Category == search.Category);
            }

            if (search.OrderId.HasValue)
            {
                query = query.Where(n => n.OrderId == search.OrderId.Value);
            }

            var createdFrom = search.CreatedFrom ?? PeriodStart(search.Period);
            if (createdFrom.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= createdFrom.Value);
            }

            if (search.CreatedTo.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= search.CreatedTo.Value);
            }

            return query;
        }

        private async Task<Notification> LoadOwnAsync(int id)
        {
            var userId = RequireUserId();

            return await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId)
                ?? throw new KeyNotFoundException($"Notification with id {id} not found.");
        }

        private static DateTime? PeriodStart(ShiftPeriod? period) => period switch
        {
            ShiftPeriod.Day => DateTime.UtcNow.AddDays(-1),
            ShiftPeriod.Week => DateTime.UtcNow.AddDays(-7),
            ShiftPeriod.Month => DateTime.UtcNow.AddMonths(-1),
            _ => null
        };

        private int RequireUserId() =>
            _userAccessor.GetUserId() ?? throw new BusinessException("The caller could not be identified.");
    }
}
