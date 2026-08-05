using PickMeUp.Model.Constants;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services;
using PickMeUp.WebAPI.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class NotificationsController
    : BaseReadController<NotificationResponse, NotificationSearch, INotificationService>
{
    public NotificationsController(INotificationService service) : base(service)
    {
    }

    /// <summary>
    /// The signed-in user's own notifications, newest first, filterable by read state, category,
    /// linked ride and date. Notifications are written by the server when a ride or a payment
    /// changes; there is no way for a client to create one, and no caller ever sees another
    /// user's notifications.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    public override Task<PageResult<NotificationResponse>> GetAll([FromQuery] NotificationSearch? search)
    {
        return base.GetAll(search);
    }

    /// <summary>
    /// One of the signed-in user's own notifications. Another user's notification is reported as
    /// not found rather than forbidden, so the response does not reveal that it exists.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    public override Task<ActionResult<NotificationResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// How many of the signed-in user's notifications are still unread. Clients use this for the
    /// badge on the notifications icon.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpGet("UnreadCount")]
    public async Task<ActionResult<UnreadCountResponse>> UnreadCount()
    {
        return Ok(await _service.GetUnreadCountAsync());
    }

    /// <summary>
    /// Marks one of the signed-in user's own notifications as read. Marking an already read
    /// notification changes nothing, and another user's notification is reported as not found.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpPost("{id}/Read")]
    public async Task<ActionResult<NotificationResponse>> Read(int id)
    {
        return Ok(await _service.MarkReadAsync(id));
    }

    /// <summary>
    /// Marks every unread notification belonging to the signed-in user as read and returns the
    /// remaining unread count, which is zero afterwards.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpPost("ReadAll")]
    public async Task<ActionResult<UnreadCountResponse>> ReadAll()
    {
        return Ok(await _service.MarkAllReadAsync());
    }
}
