namespace PickMeUp.Common.Services.Configuration
{
    public class ApiOptions
    {
        public string BasePath { get; init; } = string.Empty;

        public IReadOnlyList<string> CorsAllowedOrigins { get; init; } = Array.Empty<string>();
    }
}
