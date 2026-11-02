using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public class OrderAvailableEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string? DestinationAddress { get; set; }
        public string? CompanyName { get; set; }
        public decimal? Fare { get; set; }
        public string Currency { get; set; } = "BAM";
        public decimal? DistanceKm { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.OrderAvailable;

        public override string Subject => $"A paid ride is waiting: #{OrderId}";

        protected override EmailContent Content
        {
            get
            {
                var facts = new List<EmailFact>
                {
                    new("Ride number", $"#{OrderId}"),
                    new("Pick-up", PickupAddress),
                    new("Drop-off", DestinationAddress ?? "Not given"),
                    new("Company", CompanyName ?? "Any company driving here")
                };

                if (DistanceKm.HasValue)
                {
                    facts.Add(new EmailFact("Distance", $"{DistanceKm.Value:0.##} km"));
                }

                if (Fare.HasValue)
                {
                    facts.Add(new EmailFact("Fare", $"{Fare.Value:0.00} {Currency}"));
                }

                facts.Add(new EmailFact("Available since", OccurredAt.ToString("dd MMM yyyy HH:mm") + " UTC"));

                return new EmailContent
                {
                    Title = "A paid ride is waiting",
                    Intro = $"Hello {RecipientName}, a passenger has paid for this ride and no driver has taken it yet.",
                    Facts = facts,
                    Highlight = "Open the app and accept it if you want it. The first driver to accept gets the ride.",
                    SecurityNote =
                        "You are getting this because you are on shift and the ride is offered to your company " +
                        "in the city you drive in."
                };
            }
        }
    }
}
