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
public class ReviewsController
    : BaseCRUDController<ReviewResponse, ReviewSearch, ReviewInsertRequest, ReviewUpdateRequest, IReviewService>
{
    public ReviewsController(IReviewService service) : base(service)
    {
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    public override Task<PageResult<ReviewResponse>> GetAll([FromQuery] ReviewSearch? search)
    {
        return base.GetAll(search);
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    public override Task<ActionResult<ReviewResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// Reviews the company, the driver, or both for one of the caller's completed rides. The
    /// author is taken from the token, and a ride can be reviewed once per target.
    /// </summary>
    [Authorization(Roles.Passenger)]
    public override Task<ActionResult<ReviewResponse>> Create([FromBody] ReviewInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Passenger)]
    public override Task<ActionResult<ReviewResponse>> Update(int id, [FromBody] ReviewUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Passenger, Roles.Admin)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }

    /// <summary>
    /// Average rating and review count per taxi company, aggregated in one database query.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpGet("CompanyAverages")]
    public async Task<ActionResult<List<RatingSummaryResponse>>> CompanyAverages()
    {
        return await _service.GetCompanyAveragesAsync();
    }

    /// <summary>
    /// Average rating and review count per driver, aggregated in one database query.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    [HttpGet("DriverAverages")]
    public async Task<ActionResult<List<RatingSummaryResponse>>> DriverAverages()
    {
        return await _service.GetDriverAveragesAsync();
    }
}
