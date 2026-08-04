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
public class CitiesController
    : BaseCRUDController<CityResponse, CitySearch, CityInsertRequest, CityUpdateRequest, ICityService>
{
    public CitiesController(ICityService service) : base(service)
    {
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<CityResponse>> Create([FromBody] CityInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<CityResponse>> Update(int id, [FromBody] CityUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Admin)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }
}
