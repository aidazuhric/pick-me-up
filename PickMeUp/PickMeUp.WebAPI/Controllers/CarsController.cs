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
public class CarsController
    : BaseCRUDController<CarResponse, CarSearch, CarInsertRequest, CarUpdateRequest, ICarService>
{
    public CarsController(ICarService service) : base(service)
    {
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    public override Task<PageResult<CarResponse>> GetAll([FromQuery] CarSearch? search)
    {
        return base.GetAll(search);
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    public override Task<ActionResult<CarResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// Adds a vehicle to the calling company. The owning company is taken from the caller's
    /// token, never from the request body.
    /// </summary>
    [Authorization(Roles.Company)]
    public override Task<ActionResult<CarResponse>> Create([FromBody] CarInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Company)]
    public override Task<ActionResult<CarResponse>> Update(int id, [FromBody] CarUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Company)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }

    [Authorization(Roles.Company)]
    [HttpPost("{id}/Photo")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Choose an image file to upload." });
        }

        await using var stream = file.OpenReadStream();
        await _service.StorePhotoAsync(id, stream, file.FileName, file.ContentType);

        return NoContent();
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver)]
    [HttpGet("{id}/Photo")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var (content, contentType, fileName) = await _service.GetPhotoAsync(id);
        return File(content, contentType, fileName);
    }
}
