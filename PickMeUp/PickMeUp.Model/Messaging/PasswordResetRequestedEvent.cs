using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public class PasswordResetRequestedEvent : IntegrationEvent
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
        public int MaxAttempts { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.PasswordResetRequested;

        public override string Subject => "Reset your PickMeUp password";

        protected override EmailContent Content => new()
        {
            Title = "Reset your password",
            Intro = $"Hello {RecipientName}, enter the code below in the app to choose a new password.",
            Code = Token,
            CodeCaption = $"Expires in {ExpiresInMinutes} minute(s)",
            Facts = new List<EmailFact>
            {
                new("Valid for", $"{ExpiresInMinutes} minute(s)"),
                new("Can be used", "Once"),
                new("Attempts allowed", MaxAttempts.ToString()),
                new("Requested at", OccurredAt.ToString("dd MMM yyyy HH:mm") + " UTC")
            },
            SecurityNote =
                "Did not ask for this? Ignore this message — your current password still works " +
                "and this code will expire on its own."
        };
    }
}
