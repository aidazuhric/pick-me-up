namespace PickMeUp.Common.Services.Configuration
{
    public class TwoFactorOptions
    {
        public int CodeLength { get; init; } = 6;
        public int ExpiryMinutes { get; init; } = 5;
        public int MaxAttempts { get; init; } = 5;
        public int ResendCooldownSeconds { get; init; } = 30;
    }
}
