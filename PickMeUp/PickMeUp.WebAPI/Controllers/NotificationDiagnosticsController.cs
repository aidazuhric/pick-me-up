using PickMeUp.Model.Constants;
using PickMeUp.Model.Responses;
using PickMeUp.WebAPI.Filters;
using PickMeUp.Services.Notifications;
using PickMeUp.WebAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class NotificationDiagnosticsController : ControllerBase
{
    private readonly INotificationPublisher _publisher;
    private readonly IWebHostEnvironment _environment;

    public NotificationDiagnosticsController(INotificationPublisher publisher, IWebHostEnvironment environment)
    {
        _publisher = publisher;
        _environment = environment;
    }

    /// <summary>
    /// Pushes a throwaway notification to one user so the real-time path can be exercised.
    /// Available to administrators in development only.
    /// </summary>
    [Authorization(Roles.Admin)]
    [HttpPost("PushToUser/{userId}")]
    public async Task<IActionResult> PushToUser(int userId, [FromQuery] string? title)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var message = new NotificationMessage
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Diagnostics" : title,
            Body = "Real-time delivery check.",
            Category = "diagnostics"
        };

        await _publisher.PushToUserAsync(userId, message);

        return Ok(new { pushedTo = userId, message.Title });
    }
}
