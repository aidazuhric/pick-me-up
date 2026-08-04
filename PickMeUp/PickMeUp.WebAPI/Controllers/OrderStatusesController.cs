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
public class OrderStatusesController
    : BaseCRUDController<OrderStatusResponse, OrderStatusSearch, OrderStatusInsertRequest, OrderStatusUpdateRequest, IOrderStatusService>
{
    public OrderStatusesController(IOrderStatusService service) : base(service)
    {
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<OrderStatusResponse>> Create([FromBody] OrderStatusInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<OrderStatusResponse>> Update(int id, [FromBody] OrderStatusUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Admin)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }
}
