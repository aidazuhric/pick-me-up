using PickMeUp.Model.Access;
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

    [Authorization("Admin")]
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

    [Authorize]
    [HttpPut("ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] UserPasswordChangeRequest request)
    {
        await _service.ChangePasswordAsync(request);
        return Ok();
    }
}