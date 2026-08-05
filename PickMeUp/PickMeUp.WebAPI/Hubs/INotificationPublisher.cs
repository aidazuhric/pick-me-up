using PickMeUp.Model.Responses;

namespace PickMeUp.WebAPI.Hubs;

public interface INotificationPublisher
{
    Task PushToUserAsync(int userId, NotificationMessage message, CancellationToken cancellationToken = default);

    Task PushToUsersAsync(IEnumerable<int> userIds, NotificationMessage message, CancellationToken cancellationToken = default);
}
