namespace PickMeUp.Common.Services.Configuration
{
    public record StartupOptions(DatabaseOptions Database, JwtOptions Jwt, ApiOptions Api);
}
