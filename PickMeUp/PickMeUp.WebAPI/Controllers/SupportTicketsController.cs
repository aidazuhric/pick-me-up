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
public class SupportTicketsController
    : BaseReadController<SupportTicketResponse, SupportTicketSearch, ISupportTicketService>
{
    public SupportTicketsController(ISupportTicketService service) : base(service)
    {
    }

    /// <summary>
    /// Lists support tickets. Administrators see every ticket; everyone else sees only the
    /// ones they submitted.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    public override Task<PageResult<SupportTicketResponse>> GetAll([FromQuery] SupportTicketSearch? search)
    {
        return base.GetAll(search);
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    public override Task<ActionResult<SupportTicketResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// Files a ticket from the Online support menu. The submitter is taken from the token and
    /// the ticket always starts Open; neither can be set by the caller.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpPost]
    public async Task<ActionResult<SupportTicketResponse>> Submit([FromBody] SupportTicketInsertRequest request)
    {
        return await _service.SubmitAsync(request);
    }

    /// <summary>
    /// Adds an administrator reply. Replies are appended, never replaced, and an Open ticket
    /// moves to InProgress on the first reply.
    /// </summary>
    [Authorization(Roles.Admin)]
    [HttpPost("{id}/Replies")]
    public async Task<ActionResult<SupportTicketResponse>> Reply(int id, [FromBody] SupportTicketReplyRequest request)
    {
        return await _service.ReplyAsync(id, request);
    }

    /// <summary>
    /// Moves the ticket through its lifecycle. Resolving records who closed it and when.
    /// </summary>
    [Authorization(Roles.Admin)]
    [HttpPost("{id}/Status")]
    public async Task<ActionResult<SupportTicketResponse>> ChangeStatus(
        int id, [FromBody] SupportTicketStatusChangeRequest request)
    {
        return await _service.ChangeStatusAsync(id, request);
    }
}
