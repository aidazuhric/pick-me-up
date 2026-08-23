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
    /// One company as a passenger sees it: fleet size, rating and the price list behind a fare
    /// estimate. Read-only, and limited to companies that are currently listed.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpGet("{id}/Details")]
    public async Task<ActionResult<TaxiCompanyDetailsResponse>> Details(int id)
    {
        return await _service.GetDetailsAsync(id);
    }

    /// <summary>
    /// The signed-in company's own profile, resolved from the token. A company never passes an
    /// id, so it cannot read another company's row through this endpoint; administrators manage
    /// companies through the by-id endpoints instead.
    /// </summary>
    [Authorization(Roles.Company)]
    [HttpGet("Mine")]
    public async Task<ActionResult<TaxiCompanyResponse>> GetMine()
    {
        return await _service.GetOwnAsync();
    }

    /// <summary>
    /// Updates the signed-in company's own profile, including its price list. The company is
    /// taken from the token, so one company cannot edit another's. Whether the company is listed
    /// to passengers stays an administrator's decision and is ignored here.
    /// </summary>
    [Authorization(Roles.Company)]
    [HttpPut("Mine")]
    public async Task<ActionResult<TaxiCompanyResponse>> UpdateMine(
        [FromBody] TaxiCompanyUpdateRequest request)
    {
        return await _service.UpdateOwnAsync(request);
    }

    /// <summary>
    /// Replaces the signed-in company's own logo, with the same MIME and magic-byte checks as the
    /// administrator upload.
    /// </summary>
    [Authorization(Roles.Company)]
    [HttpPost("Mine/Logo")]
    public async Task<IActionResult> UploadOwnLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Choose an image file to upload." });
        }

        await using var stream = file.OpenReadStream();
        await _service.StoreOwnLogoAsync(stream, file.FileName, file.ContentType);

        return NoContent();
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
