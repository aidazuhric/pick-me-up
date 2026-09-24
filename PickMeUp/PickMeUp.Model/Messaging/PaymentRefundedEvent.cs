using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public class PaymentRefundedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BAM";
        public string PickupAddress { get; set; } = string.Empty;
        public string? DestinationAddress { get; set; }
        public string? CompanyName { get; set; }
        public string? Reason { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.PaymentRefunded;

        public override string Subject => $"Refund issued for ride #{OrderId}";

        protected override EmailContent Content
        {
            get
            {
                var facts = new List<EmailFact>
                {
                    new("Ride number", $"#{OrderId}"),
                    new("Amount refunded", $"{Amount:0.00} {Currency}"),
                    new("From", PickupAddress),
                    new("To", DestinationAddress ?? "Not given")
                };

                if (!string.IsNullOrWhiteSpace(CompanyName))
                {
                    facts.Add(new EmailFact("Company", CompanyName!));
                }

                if (!string.IsNullOrWhiteSpace(Reason))
                {
                    facts.Add(new EmailFact("Reason", Reason!));
                }

                facts.Add(new EmailFact("Refunded at", OccurredAt.ToString("dd MMM yyyy HH:mm") + " UTC"));

                return new EmailContent
                {
                    Title = "Your refund is on its way",
                    Intro = $"Hello {RecipientName}, we have sent your money back.",
                    Facts = facts,
                    Highlight = "Your bank usually shows it within a few working days.",
                    SecurityNote =
                        "Nothing further is owed for this ride. If the amount has not appeared " +
                        "after a week, contact support from the app."
                };
            }
        }
    }
}
