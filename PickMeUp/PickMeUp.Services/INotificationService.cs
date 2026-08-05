using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface INotificationService : IBaseReadService<NotificationResponse, NotificationSearch>
    {
        Task<UnreadCountResponse> GetUnreadCountAsync();

        Task<NotificationResponse> MarkReadAsync(int id);

        Task<UnreadCountResponse> MarkAllReadAsync();
    }
}
