using PickMeUp.Model.Constants;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services;
using PickMeUp.WebAPI.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : BaseReadController<OrderResponse, OrderSearch, IOrderService>
{
    public OrdersController(IOrderService service) : base(service)
    {
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    public override Task<PageResult<OrderResponse>> GetAll([FromQuery] OrderSearch? search)
    {
        return base.GetAll(search);
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    public override Task<ActionResult<OrderResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// Places a pending order. The fare is never accepted from the caller — the server
    /// computes it from the company's price list once the ride completes.
    /// </summary>
    [Authorization(Roles.Passenger)]
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] OrderInsertRequest request)
    {
        return await _service.CreateAsync(request);
    }

    /// <summary>
    /// A dispatch-eligible driver takes the order, which moves it from Pending to Confirmed
    /// and binds the driver and their assigned vehicle to it.
    /// </summary>
    [Authorization(Roles.Driver)]
    [HttpPost("{id}/Accept")]
    public async Task<ActionResult<OrderResponse>> Accept(int id, [FromBody] OrderAcceptRequest? request)
    {
        return await _service.AcceptAsync(id, request ?? new OrderAcceptRequest());
    }

    /// <summary>
    /// Declines the order for this driver only. The order stays Pending and remains offered
    /// to every other eligible driver; it is not cancelled.
    /// </summary>
    [Authorization(Roles.Driver)]
    [HttpPost("{id}/Reject")]
    public async Task<ActionResult<OrderResponse>> Reject(int id)
    {
        return await _service.RejectAsync(id);
    }

    [Authorization(Roles.Driver)]
    [HttpPost("{id}/Pickup")]
    public async Task<ActionResult<OrderResponse>> Pickup(int id)
    {
        return await _service.PickupAsync(id);
    }

    /// <summary>
    /// Finishes the ride, prices it from the company's list, and counts it against the
    /// driver's open shift.
    /// </summary>
    [Authorization(Roles.Driver)]
    [HttpPost("{id}/Complete")]
    public async Task<ActionResult<OrderResponse>> Complete(int id, [FromBody] OrderCompleteRequest? request)
    {
        return await _service.CompleteAsync(id, request ?? new OrderCompleteRequest());
    }

    /// <summary>
    /// Cancels the order. A reason is mandatory and is stored on the audit trail.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Passenger, Roles.Driver)]
    [HttpPost("{id}/Cancel")]
    public async Task<ActionResult<OrderResponse>> Cancel(int id, [FromBody] OrderCancelRequest request)
    {
        return await _service.CancelAsync(id, request);
    }
}
