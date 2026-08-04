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
public class VehicleCategoriesController
    : BaseCRUDController<VehicleCategoryResponse, VehicleCategorySearch, VehicleCategoryInsertRequest, VehicleCategoryUpdateRequest, IVehicleCategoryService>
{
    public VehicleCategoriesController(IVehicleCategoryService service) : base(service)
    {
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<VehicleCategoryResponse>> Create([FromBody] VehicleCategoryInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<VehicleCategoryResponse>> Update(int id, [FromBody] VehicleCategoryUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Admin)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }
}
