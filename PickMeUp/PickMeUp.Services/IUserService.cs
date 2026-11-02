using PickMeUp.Model.Access;
using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface IUserService : IBaseCRUDService<UserResponse, UserSearch, UserInsertRequest, UserUpdateRequest>
    {
        Task<UserSensitiveResponse?> GetByUsernameAsync(string username);
        Task<UserResponse?> GetWithRoleByIdAsync(int id);

        Task MarkSignedInAsync(int userId);
        Task ChangePasswordAsync(UserPasswordChangeRequest request);

        Task<UserResponse> GetOwnAsync();

        Task<UserResponse> UpdateOwnAsync(UserProfileUpdateRequest request);

        Task DeleteOwnAccountAsync();

        Task<string> StoreOwnProfileImageAsync(Stream content, string fileName, string contentType);

        Task<string> StoreProfileImageAsync(int id, Stream content, string fileName, string contentType);

        Task<(Stream Content, string ContentType, string FileName)> GetProfileImageAsync(int id);
    }
}
