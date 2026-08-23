using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;

namespace PickMeUp.Services
{
    public interface IPasswordResetService
    {
        Task<PasswordResetResponse> RequestAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

        Task ResetAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    }
}
