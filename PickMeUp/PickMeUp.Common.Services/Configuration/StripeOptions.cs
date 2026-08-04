namespace PickMeUp.Common.Services.Configuration
{
    public class StripeOptions
    {
        public string SecretKey { get; init; } = string.Empty;

        public string WebhookSecret { get; init; } = string.Empty;
    }
}
