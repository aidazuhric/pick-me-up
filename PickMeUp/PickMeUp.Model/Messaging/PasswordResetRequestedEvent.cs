namespace PickMeUp.Model.Messaging
{
    public class PasswordResetRequestedEvent : IntegrationEvent
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.PasswordResetRequested;

        public override string Subject => "Reset your PickMeUp password";

        public override string Body =>
            $"Your password reset code is {Token}. It works once and stops working in " +
            $"{ExpiresInMinutes} minute(s). If you did not ask to reset your password, " +
            "ignore this message — your current password still works.";
    }
}
