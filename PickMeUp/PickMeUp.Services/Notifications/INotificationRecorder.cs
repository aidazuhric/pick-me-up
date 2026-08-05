using PickMeUp.Model.Responses;

namespace PickMeUp.Services.Notifications
{
    public interface INotificationRecorder
    {
        Task DeliverAsync(int userId, NotificationMessage message, CancellationToken cancellationToken = default);

        Task DeliverAsync(IEnumerable<int> userIds, NotificationMessage message, CancellationToken cancellationToken = default);
    }
}
