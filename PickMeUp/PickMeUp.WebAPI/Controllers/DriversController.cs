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
public class DriversController
    : BaseCRUDController<DriverResponse, DriverSearch, DriverInsertRequest, DriverUpdateRequest, IDriverService>
{
    public DriversController(IDriverService service) : base(service)
    {
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    public override Task<PageResult<DriverResponse>> GetAll([FromQuery] DriverSearch? search)
    {
        return base.GetAll(search);
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    public override Task<ActionResult<DriverResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// Adds a driver to the calling company and provisions their Driver-role login in one
    /// transaction. The owning company is taken from the caller's token, never from the body.
    /// </summary>
    [Authorization(Roles.Company)]
    public override Task<ActionResult<DriverResponse>> Create([FromBody] DriverInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Company)]
    public override Task<ActionResult<DriverResponse>> Update(int id, [FromBody] DriverUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Company)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }

    /// <summary>
    /// Records the driver's resignation. This is the reversible counterpart to deletion —
    /// the employment record is kept and the driver stops receiving work.
    /// </summary>
    [Authorization(Roles.Company, Roles.Driver)]
    [HttpPost("{id}/Resign")]
    public async Task<ActionResult<DriverResponse>> Resign(int id)
    {
        return await _service.ResignAsync(id);
    }
}
