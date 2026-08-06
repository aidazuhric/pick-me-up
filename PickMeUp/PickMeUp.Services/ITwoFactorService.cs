using PickMeUp.Model.Access;

namespace PickMeUp.Services
{
    public interface ITwoFactorService
    {
        Task<TwoFactorChallenge> IssueChallengeAsync(int userId, CancellationToken cancellationToken = default);

        Task<int> VerifyAsync(int userId, string code, CancellationToken cancellationToken = default);

        Task<TwoFactorSettingResponse> GetSettingAsync(int userId);

        Task<TwoFactorSettingResponse> SetEnabledAsync(int userId, bool enabled);

        string MaskEmail(string email);
    }

    public record TwoFactorChallenge(string MaskedEmail, int ExpiresInSeconds, bool CodeSent);
}
