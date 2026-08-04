using PickMeUp.Model.Constants;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services;
using PickMeUp.WebAPI.Filters;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ShiftsController : BaseReadController<ShiftResponse, ShiftSearch, IShiftService>
{
    public ShiftsController(IShiftService service) : base(service)
    {
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    public override Task<PageResult<ShiftResponse>> GetAll([FromQuery] ShiftSearch? search)
    {
        return base.GetAll(search);
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    public override Task<ActionResult<ShiftResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// Opens a shift for the calling driver. A driver may hold only one open shift at a time.
    /// </summary>
    [Authorization(Roles.Driver)]
    [HttpPost("Start")]
    public async Task<ActionResult<ShiftResponse>> Start()
    {
        return await _service.StartShiftAsync();
    }

    /// <summary>
    /// Closes the shift. If the driver is still on a break, that break is closed at the same
    /// moment rather than the request being refused.
    /// </summary>
    [Authorization(Roles.Driver)]
    [HttpPost("{id}/Stop")]
    public async Task<ActionResult<ShiftResponse>> Stop(int id)
    {
        return await _service.StopShiftAsync(id);
    }

    [Authorization(Roles.Driver)]
    [HttpPost("{id}/Break/Start")]
    public async Task<ActionResult<ShiftResponse>> StartBreak(int id)
    {
        return await _service.StartBreakAsync(id);
    }

    [Authorization(Roles.Driver)]
    [HttpPost("{id}/Break/Stop")]
    public async Task<ActionResult<ShiftResponse>> StopBreak(int id)
    {
        return await _service.StopBreakAsync(id);
    }

    /// <summary>
    /// Whether the driver may currently receive orders: they must have an open shift and not
    /// be on a break. Order dispatch reads from this rule.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    [HttpGet("Eligibility/{driverId}")]
    public async Task<ActionResult<DriverEligibilityResponse>> Eligibility(int driverId)
    {
        return await _service.GetEligibilityAsync(driverId);
    }
}
