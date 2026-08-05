using PickMeUp.Model.Responses;

namespace PickMeUp.Services.Notifications
{
    public interface INotificationPublisher
    {
        Task PushToUserAsync(int userId, NotificationMessage message, CancellationToken cancellationToken = default);

        Task PushToUsersAsync(IEnumerable<int> userIds, NotificationMessage message, CancellationToken cancellationToken = default);
    }
}
