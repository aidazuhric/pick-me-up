namespace PickMeUp.Services;

public interface IAuthenticatedUserAccessor
{
    int? GetUserId();

    bool IsInRole(string role);
}
