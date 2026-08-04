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
public class SupportTicketStatusesController
    : BaseCRUDController<SupportTicketStatusResponse, SupportTicketStatusSearch, SupportTicketStatusInsertRequest, SupportTicketStatusUpdateRequest, ISupportTicketStatusService>
{
    public SupportTicketStatusesController(ISupportTicketStatusService service) : base(service)
    {
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<SupportTicketStatusResponse>> Create([FromBody] SupportTicketStatusInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<SupportTicketStatusResponse>> Update(int id, [FromBody] SupportTicketStatusUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Admin)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }
}
