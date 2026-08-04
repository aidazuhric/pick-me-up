using PickMeUp.Model.Access;
using PickMeUp.Services;
using PickMeUp.WebAPI.Services.AccessManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAccessManager _accessManager;
    private readonly IAuthenticatedUserAccessor _authenticatedUserAccessor;

    public AuthController(IAccessManager accessManager, IAuthenticatedUserAccessor authenticatedUserAccessor)
    {
        _accessManager = accessManager;
        _authenticatedUserAccessor = authenticatedUserAccessor;
    }

    [HttpPost("Login")]
    public async Task<ActionResult<UserLoginResponse>> Login([FromBody] UserLoginRequest request)
    {
        var result = await _accessManager.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("RefreshToken")]
    public async Task<ActionResult<UserLoginResponse>> RefreshToken([FromBody] RefreshAccessTokenRequest request)
    {
        var result = await _accessManager.LoginWithRefreshTokenAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Revokes every refresh token issued to the caller, so the session cannot be renewed
    /// after the client discards its access token. The caller is taken from the JWT.
    /// </summary>
    [Authorize]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = _authenticatedUserAccessor.GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        await _accessManager.LogoutAsync(userId.Value);
        return NoContent();
    }
}
