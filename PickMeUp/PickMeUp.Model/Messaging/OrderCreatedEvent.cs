using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public class OrderCreatedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string? DestinationAddress { get; set; }
        public string? CompanyName { get; set; }
        public decimal? Fare { get; set; }
        public string Currency { get; set; } = "BAM";
        public decimal? DistanceKm { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.OrderCreated;

        public override string Subject => $"Ride #{OrderId} is booked";

        protected override EmailContent Content
        {
            get
            {
                var facts = new List<EmailFact>
                {
                    new("Ride number", $"#{OrderId}"),
                    new("From", PickupAddress),
                    new("To", DestinationAddress ?? "Not given"),
                    new("Company", CompanyName ?? "Any company driving here")
                };

                if (DistanceKm.HasValue)
                {
                    facts.Add(new EmailFact("Distance", $"{DistanceKm.Value:0.##} km"));
                }

                if (Fare.HasValue)
                {
                    facts.Add(new EmailFact("Price", $"{Fare.Value:0.00} {Currency}"));
                }

                facts.Add(new EmailFact("Booked at", OccurredAt.ToString("dd MMM yyyy HH:mm") + " UTC"));

                return new EmailContent
                {
                    Title = "Your ride is booked",
                    Intro = $"Hello {RecipientName}, we have your ride. Here is what you asked for.",
                    Facts = facts,
                    Highlight = "Pay for the ride so drivers can see it and accept.",
                    SecurityNote =
                        "You can follow the ride, or cancel it for a refund while no driver has taken it, " +
                        "from the Ride status screen in the app."
                };
            }
        }
    }
}
