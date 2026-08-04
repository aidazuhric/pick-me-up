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
public class TaxiCompaniesController
    : BaseCRUDController<TaxiCompanyResponse, TaxiCompanySearch, TaxiCompanyInsertRequest, TaxiCompanyUpdateRequest, ITaxiCompanyService>
{
    public TaxiCompaniesController(ITaxiCompanyService service) : base(service)
    {
    }

    /// <summary>
    /// Creates a taxi company together with its initial Company-role login, in one transaction.
    /// When no password is supplied a temporary one is generated and returned once, on this
    /// response only — it is never exposed by any later read.
    /// </summary>
    [Authorization(Roles.Admin)]
    public override Task<ActionResult<TaxiCompanyResponse>> Create([FromBody] TaxiCompanyInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorization(Roles.Admin)]
    public override Task<ActionResult<TaxiCompanyResponse>> Update(int id, [FromBody] TaxiCompanyUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Admin)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }

    /// <summary>
    /// Replaces the company logo. The upload is accepted only when the declared MIME type and
    /// the file's own magic bytes agree on JPEG or PNG.
    /// </summary>
    [Authorization(Roles.Admin)]
    [HttpPost("{id}/Logo")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Choose an image file to upload." });
        }

        await using var stream = file.OpenReadStream();
        await _service.StoreLogoAsync(id, stream, file.FileName, file.ContentType);

        return NoContent();
    }

    [HttpGet("{id}/Logo")]
    public async Task<IActionResult> GetLogo(int id)
    {
        var (content, contentType, fileName) = await _service.GetLogoAsync(id);
        return File(content, contentType, fileName);
    }
}
