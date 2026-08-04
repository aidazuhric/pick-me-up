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
public class CarAssignmentsController
    : BaseCRUDController<CarAssignmentResponse, CarAssignmentSearch, CarAssignmentInsertRequest, CarAssignmentUpdateRequest, ICarAssignmentService>
{
    public CarAssignmentsController(ICarAssignmentService service) : base(service)
    {
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    public override Task<PageResult<CarAssignmentResponse>> GetAll([FromQuery] CarAssignmentSearch? search)
    {
        return base.GetAll(search);
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    public override Task<ActionResult<CarAssignmentResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// Assigns one of the calling company's vehicles to one of its drivers. The assignment
    /// window may not overlap another active assignment of the same car or the same driver.
    /// </summary>
    [Authorization(Roles.Company)]
    public override Task<ActionResult<CarAssignmentResponse>> Create([FromBody] CarAssignmentInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Company)]
    public override Task<ActionResult<CarAssignmentResponse>> Update(int id, [FromBody] CarAssignmentUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Company)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }

    /// <summary>
    /// Ends an open-ended assignment as of now, freeing the car and the driver for reassignment.
    /// </summary>
    [Authorization(Roles.Company)]
    [HttpPost("{id}/Close")]
    public async Task<ActionResult<CarAssignmentResponse>> Close(int id)
    {
        return await _service.CloseAsync(id);
    }
}
