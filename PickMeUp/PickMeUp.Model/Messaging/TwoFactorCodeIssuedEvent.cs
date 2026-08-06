namespace PickMeUp.Model.Messaging
{
    public class TwoFactorCodeIssuedEvent : IntegrationEvent
    {
        public string Code { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.TwoFactorCodeIssued;

        public override string Subject => "Your PickMeUp sign-in code";

        public override string Body =>
            $"Your sign-in code is {Code}. It stops working in {ExpiresInMinutes} minute(s). " +
            "If you did not try to sign in, ignore this message and change your password.";
    }
}
