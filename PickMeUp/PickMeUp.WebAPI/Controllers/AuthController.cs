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
    private readonly ITwoFactorService _twoFactorService;

    public AuthController(IAccessManager accessManager, IAuthenticatedUserAccessor authenticatedUserAccessor, ITwoFactorService twoFactorService)
    {
        _accessManager = accessManager;
        _authenticatedUserAccessor = authenticatedUserAccessor;
        _twoFactorService = twoFactorService;
    }

    /// <summary>
    /// Checks the credentials. An account with two-factor authentication turned on gets no
    /// token here — the response only reports that a code was sent, and the token is issued
    /// by <c>Verify</c> once that code is presented.
    /// </summary>
    [HttpPost("Login")]
    public async Task<ActionResult<UserLoginResponse>> Login([FromBody] UserLoginRequest request)
    {
        var result = await _accessManager.LoginAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Exchanges the emailed code for a token. The code is single use and expires.
    /// </summary>
    [HttpPost("Verify")]
    public async Task<ActionResult<UserLoginResponse>> Verify([FromBody] TwoFactorVerifyRequest request)
    {
        var result = await _accessManager.VerifyTwoFactorAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Reports whether the caller signs in with an emailed code, and to which address.
    /// </summary>
    [Authorize]
    [HttpGet("TwoFactor")]
    public async Task<ActionResult<TwoFactorSettingResponse>> GetTwoFactor()
    {
        var userId = _authenticatedUserAccessor.GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _twoFactorService.GetSettingAsync(userId.Value));
    }

    /// <summary>
    /// Turns the emailed sign-in code on or off for the caller's own account.
    /// </summary>
    [Authorize]
    [HttpPut("TwoFactor")]
    public async Task<ActionResult<TwoFactorSettingResponse>> SetTwoFactor([FromBody] TwoFactorSettingRequest request)
    {
        var userId = _authenticatedUserAccessor.GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(await _twoFactorService.SetEnabledAsync(userId.Value, request.Enabled));
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
