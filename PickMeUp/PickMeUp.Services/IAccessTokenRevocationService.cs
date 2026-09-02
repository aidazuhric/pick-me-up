namespace PickMeUp.Services
{
    public interface IAccessTokenRevocationService
    {
        Task RevokeAsync(string tokenId, int userId, DateTime expiresAt, CancellationToken cancellationToken = default);

        Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken = default);
    }
}
