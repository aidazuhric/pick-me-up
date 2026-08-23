using PickMeUp.Model.Access;
using PickMeUp.Model.Constants;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services;
using PickMeUp.WebAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : BaseCRUDController<UserResponse, UserSearch, UserInsertRequest, UserUpdateRequest, IUserService>
{
    public UsersController(IUserService userService) : base(userService)
    {
    }

    [Authorization(Roles.Admin)]
    public override Task<PageResult<UserResponse>> GetAll([FromQuery] UserSearch? search)
    {
        return base.GetAll(search);
    }

    /// <summary>
    /// Self-registration — the one write endpoint that must stay open, since a caller
    /// creating an account has no token yet.
    /// </summary>
    [AllowAnonymous]
    public override Task<ActionResult<UserResponse>> Create([FromBody] UserInsertRequest request)
    {
        return base.Create(request);
    }

    /// <summary>
    /// The signed-in account, resolved from the token. A caller never passes an id, so one user
    /// cannot read another's profile through this endpoint.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpGet("Me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        return await _service.GetOwnAsync();
    }

    /// <summary>
    /// Updates the signed-in account's own name, email and phone. The account is taken from the
    /// token; role, active state and username are not editable here.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpPut("Me")]
    public async Task<ActionResult<UserResponse>> UpdateMe([FromBody] UserProfileUpdateRequest request)
    {
        return await _service.UpdateOwnAsync(request);
    }

    /// <summary>
    /// Replaces the signed-in account's own photo.
    /// </summary>
    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpPost("Me/Photo")]
    public async Task<IActionResult> UploadOwnPhoto(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Choose an image file to upload." });
        }

        await using var stream = file.OpenReadStream();
        await _service.StoreOwnProfileImageAsync(stream, file.FileName, file.ContentType);

        return NoContent();
    }

    /// <summary>
    /// Reads any account. Administrators only — everyone else reads their own through <c>Me</c>.
    /// </summary>
    [Authorization(Roles.Admin)]
    public override Task<ActionResult<UserResponse>> GetById(int id)
    {
        return base.GetById(id);
    }

    /// <summary>
    /// Edits any account. Administrators only — everyone else edits their own through <c>Me</c>.
    /// </summary>
    [Authorization(Roles.Admin)]
    public override Task<ActionResult<UserResponse>> Update(int id, [FromBody] UserUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorization(Roles.Admin)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }

    /// <summary>
    /// Changes a password. A caller who is not an administrator can only change their own, so the
    /// id in the body is ignored for them and the account is taken from the token.
    /// </summary>
    [Authorize]
    [HttpPut("ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] UserPasswordChangeRequest request)
    {
        await _service.ChangePasswordAsync(request);
        return Ok();
    }

    [Authorization(Roles.Admin)]
    [HttpPost("{id}/Photo")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Choose an image file to upload." });
        }

        await using var stream = file.OpenReadStream();
        await _service.StoreProfileImageAsync(id, stream, file.FileName, file.ContentType);

        return NoContent();
    }

    [Authorization(Roles.Admin, Roles.Company, Roles.Driver, Roles.Passenger)]
    [HttpGet("{id}/Photo")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var (content, contentType, fileName) = await _service.GetProfileImageAsync(id);
        return File(content, contentType, fileName);
    }
}