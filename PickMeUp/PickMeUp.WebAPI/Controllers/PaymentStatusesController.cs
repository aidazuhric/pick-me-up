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
public class PaymentStatusesController
    : BaseCRUDController<PaymentStatusResponse, PaymentStatusSearch, PaymentStatusInsertRequest, PaymentStatusUpdateRequest, IPaymentStatusService>
{
    public PaymentStatusesController(IPaymentStatusService service) : base(service)
    {
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<PaymentStatusResponse>> Create([FromBody] PaymentStatusInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<PaymentStatusResponse>> Update(int id, [FromBody] PaymentStatusUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Admin)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }
}
