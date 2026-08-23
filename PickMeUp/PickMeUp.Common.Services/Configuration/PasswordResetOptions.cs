namespace PickMeUp.Common.Services.Configuration
{
    public class PasswordResetOptions
    {
        public int TokenLength { get; init; } = 8;

        public int ExpiryMinutes { get; init; } = 20;

        public int MaxAttempts { get; init; } = 5;
    }
}
