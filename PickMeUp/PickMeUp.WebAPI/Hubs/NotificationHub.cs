using PickMeUp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PickMeUp.WebAPI.Hubs;

/// <summary>
/// Real-time notification channel. The socket must carry a valid JWT; the caller is placed in
/// a group keyed by their own user id, so a push reaches only that user's connections.
/// Clients listen for the <c>ReceiveNotification</c> event.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    public const string ReceiveNotification = "ReceiveNotification";

    private readonly IAuthenticatedUserAccessor _userAccessor;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(IAuthenticatedUserAccessor userAccessor, ILogger<NotificationHub> logger)
    {
        _userAccessor = userAccessor;
        _logger = logger;
    }

    public static string GroupFor(int userId) => $"user-{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = _userAccessor.GetUserId();

        if (userId == null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(userId.Value));
        _logger.LogInformation("User {UserId} connected to notifications", userId.Value);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _userAccessor.GetUserId();

        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupFor(userId.Value));
        }

        await base.OnDisconnectedAsync(exception);
    }
}
