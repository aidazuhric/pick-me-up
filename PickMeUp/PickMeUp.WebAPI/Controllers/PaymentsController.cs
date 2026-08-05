using PickMeUp.Model.Constants;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services;
using PickMeUp.WebAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentsController : BaseReadController<PaymentResponse, PaymentSearch, IPaymentService>
{
    public PaymentsController(IPaymentService service) : base(service)
    {
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Passenger)]
    public override Task<PageResult<PaymentResponse>> GetAll([FromQuery] PaymentSearch? search)
    {
        return base.GetAll(search);
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Passenger)]
    public override Task<ActionResult<PaymentResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// Payment status and refund history for one ride. A company may read this only for its own
    /// orders, and has no way to change the amount or the status — those are set by Stripe
    /// confirmation alone.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Passenger)]
    [HttpGet("ByOrder/{orderId}")]
    public async Task<ActionResult<PaymentResponse>> GetByOrder(int orderId)
    {
        return await _service.GetByOrderAsync(orderId);
    }

    /// <summary>
    /// Starts payment for a completed ride. The amount is taken from the fare the server
    /// computed when the ride finished — the caller cannot supply or influence it.
    /// </summary>
    [Authorization(Roles.Passenger, Roles.Admin)]
    [HttpPost("Intent")]
    public async Task<ActionResult<PaymentIntentResponse>> CreateIntent([FromBody] PaymentIntentRequest request)
    {
        return await _service.CreateIntentAsync(request);
    }

    /// <summary>
    /// The only path by which a payment becomes paid. The request body is verified against the
    /// Stripe signing secret before anything is recorded, and replaying an already-confirmed
    /// event has no further effect.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("Webhook")]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var applied = await _service.HandleWebhookAsync(payload, signature);

        return Ok(new { applied });
    }

    /// <summary>
    /// Refunds against the amount actually charged, never a recomputed fare. Omitting the amount
    /// refunds everything still outstanding.
    /// </summary>
    [Authorization(Roles.Admin)]
    [HttpPost("{id}/Refund")]
    public async Task<ActionResult<PaymentResponse>> Refund(int id, [FromBody] PaymentRefundRequest? request)
    {
        return await _service.RefundAsync(id, request ?? new PaymentRefundRequest());
    }
}
