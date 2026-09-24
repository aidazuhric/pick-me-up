using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public class PaymentConfirmedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BAM";
        public string PickupAddress { get; set; } = string.Empty;
        public string? DestinationAddress { get; set; }
        public decimal? DistanceKm { get; set; }
        public string? CompanyName { get; set; }
        public string? DriverName { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.PaymentConfirmed;

        public override string Subject => $"Payment received for ride #{OrderId}";

        protected override EmailContent Content
        {
            get
            {
                var facts = new List<EmailFact>
                {
                    new("Ride number", $"#{OrderId}"),
                    new("Amount paid", $"{Amount:0.00} {Currency}"),
                    new("From", PickupAddress),
                    new("To", DestinationAddress ?? "Not given")
                };

                if (DistanceKm.HasValue)
                {
                    facts.Add(new EmailFact("Distance", $"{DistanceKm.Value:0.##} km"));
                }

                if (!string.IsNullOrWhiteSpace(CompanyName))
                {
                    facts.Add(new EmailFact("Company", CompanyName!));
                }

                if (!string.IsNullOrWhiteSpace(DriverName))
                {
                    facts.Add(new EmailFact("Driver", DriverName!));
                }

                facts.Add(new EmailFact("Paid at", OccurredAt.ToString("dd MMM yyyy HH:mm") + " UTC"));

                return new EmailContent
                {
                    Title = "Payment received",
                    Intro = $"Thank you {RecipientName}, your payment went through.",
                    Facts = facts,
                    Highlight = "Waiting for a driver to accept your ride.",
                    SecurityNote =
                        "Keep this message as your receipt. If no driver takes the ride you can " +
                        "cancel it in the app and the amount comes straight back."
                };
            }
        }
    }
}
