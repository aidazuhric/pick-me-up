using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public class TwoFactorCodeIssuedEvent : IntegrationEvent
    {
        public string Code { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
        public int MaxAttempts { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.TwoFactorCodeIssued;

        public override string Subject => "Your PickMeUp sign-in code";

        protected override EmailContent Content => new()
        {
            Title = "Your sign-in code",
            Intro = $"Hello {RecipientName}, use the code below to finish signing in to PickMeUp.",
            Code = Code,
            CodeCaption = $"Expires in {ExpiresInMinutes} minute(s)",
            Facts = new List<EmailFact>
            {
                new("Valid for", $"{ExpiresInMinutes} minute(s)"),
                new("Attempts allowed", MaxAttempts.ToString()),
                new("Requested at", OccurredAt.ToString("dd MMM yyyy HH:mm") + " UTC")
            },
            SecurityNote =
                "If you did not try to sign in, ignore this message and change your password. " +
                "Nobody from PickMeUp will ever ask you for this code."
        };
    }
}
