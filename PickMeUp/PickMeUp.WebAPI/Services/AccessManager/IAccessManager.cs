using PickMeUp.Model.Access;

namespace PickMeUp.WebAPI.Services.AccessManager
{
    public interface IAccessManager
    {
        Task<UserLoginResponse> LoginAsync(UserLoginRequest request);
        Task<UserLoginResponse> LoginWithRefreshTokenAsync(RefreshAccessTokenRequest request);
    }
}
