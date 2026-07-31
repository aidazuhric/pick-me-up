using PickMeUp.Model.Access;
using PickMeUp.WebAPI.Services.AccessManager;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAccessManager _accessManager;

    public AuthController(IAccessManager accessManager)
    {
        _accessManager = accessManager;
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
}
