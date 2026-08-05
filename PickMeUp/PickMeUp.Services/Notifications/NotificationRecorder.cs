using PickMeUp.Model.Responses;
using PickMeUp.Services.Database;
using Microsoft.Extensions.Logging;

namespace PickMeUp.Services.Notifications
{
    public class NotificationRecorder : INotificationRecorder
    {
        private readonly PickMeUpDbContext _dbContext;
        private readonly INotificationPublisher _publisher;
        private readonly ILogger<NotificationRecorder> _logger;

        public NotificationRecorder(
            PickMeUpDbContext dbContext,
            INotificationPublisher publisher,
            ILogger<NotificationRecorder> logger)
        {
            _dbContext = dbContext;
            _publisher = publisher;
            _logger = logger;
        }

        public Task DeliverAsync(int userId, NotificationMessage message, CancellationToken cancellationToken = default) =>
            DeliverAsync(new[] { userId }, message, cancellationToken);

        public async Task DeliverAsync(
            IEnumerable<int> userIds, NotificationMessage message, CancellationToken cancellationToken = default)
        {
            var recipients = userIds.Distinct().ToList();

            if (recipients.Count == 0)
            {
                return;
            }

            var persisted = await PersistAsync(recipients, message, cancellationToken);

            foreach (var userId in recipients)
            {
                var payload = new NotificationMessage
                {
                    NotificationId = persisted.TryGetValue(userId, out var id) ? id : null,
                    Title = message.Title,
                    Body = message.Body,
                    Category = message.Category,
                    OrderId = message.OrderId,
                    IsRead = false,
                    CreatedAt = message.CreatedAt
                };

                await _publisher.PushToUserAsync(userId, payload, cancellationToken);
            }
        }

        private async Task<Dictionary<int, int>> PersistAsync(
            IReadOnlyCollection<int> recipients, NotificationMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var rows = recipients.Select(userId => new Notification
                {
                    UserId = userId,
                    Title = message.Title,
                    Body = message.Body,
                    Category = message.Category ?? string.Empty,
                    OrderId = message.OrderId,
                    IsRead = false,
                    CreatedAt = message.CreatedAt
                }).ToList();

                _dbContext.Notifications.AddRange(rows);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return rows.ToDictionary(row => row.UserId, row => row.Id);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Could not store notification '{Title}' for {RecipientCount} recipient(s). Delivery continues.",
                    message.Title, recipients.Count);

                return new Dictionary<int, int>();
            }
        }
    }
}
