using PickMeUp.Model.Responses;
using Microsoft.AspNetCore.SignalR;

namespace PickMeUp.WebAPI.Hubs;

public class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationPublisher> _logger;

    public SignalRNotificationPublisher(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationPublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task PushToUserAsync(int userId, NotificationMessage message, CancellationToken cancellationToken = default) =>
        SendAsync(NotificationHub.GroupFor(userId), message, cancellationToken);

    public async Task PushToUsersAsync(IEnumerable<int> userIds, NotificationMessage message, CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds.Distinct())
        {
            await PushToUserAsync(userId, message, cancellationToken);
        }
    }

    private async Task SendAsync(string group, NotificationMessage message, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(group)
            .SendAsync(NotificationHub.ReceiveNotification, message, cancellationToken);

        _logger.LogInformation("Pushed notification to {Group}: {Title}", group, message.Title);
    }
}
