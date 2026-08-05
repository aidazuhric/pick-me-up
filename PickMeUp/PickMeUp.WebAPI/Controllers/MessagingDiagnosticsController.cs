using PickMeUp.Model.Constants;
using PickMeUp.Model.Messaging;
using PickMeUp.Services.Messaging;
using PickMeUp.WebAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MessagingDiagnosticsController : ControllerBase
{
    private readonly IEventPublisher _publisher;
    private readonly IWebHostEnvironment _environment;

    public MessagingDiagnosticsController(IEventPublisher publisher, IWebHostEnvironment environment)
    {
        _publisher = publisher;
        _environment = environment;
    }

    /// <summary>
    /// Publishes a throwaway event so the messaging path can be exercised without waiting for a
    /// real order. Available to administrators in development only.
    /// </summary>
    [Authorization(Roles.Admin)]
    [HttpPost("PublishTestEvent")]
    public async Task<IActionResult> PublishTestEvent([FromQuery] string? email)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var integrationEvent = new OrderStatusChangedEvent
        {
            OrderId = 0,
            FromStatus = "Pending",
            ToStatus = "Confirmed",
            DriverName = "Diagnostics",
            RecipientEmail = string.IsNullOrWhiteSpace(email) ? "diagnostics@pickmeup.local" : email,
            RecipientName = "Diagnostics"
        };

        await _publisher.PublishAsync(integrationEvent);

        return Ok(new { published = integrationEvent.EventId, routingKey = integrationEvent.RoutingKey });
    }
}
