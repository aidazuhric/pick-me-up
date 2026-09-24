using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public class OrderStatusChangedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string FromStatus { get; set; } = string.Empty;
        public string ToStatus { get; set; } = string.Empty;
        public string? DriverName { get; set; }
        public string? CompanyName { get; set; }
        public string? CarPlate { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string? DestinationAddress { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.OrderStatusChanged;

        public override string Subject => $"Ride #{OrderId} is now {ToStatus}";

        private string Headline => ToStatus switch
        {
            "Confirmed" => "A driver is on the way",
            "InProgress" => "You are on your way",
            "Completed" => "Your ride is finished",
            "Cancelled" => "Your ride was cancelled",
            _ => $"Your ride is now {ToStatus}"
        };

        protected override EmailContent Content
        {
            get
            {
                var facts = new List<EmailFact>
                {
                    new("Ride number", $"#{OrderId}"),
                    new("Status", $"{FromStatus} → {ToStatus}"),
                    new("From", PickupAddress),
                    new("To", DestinationAddress ?? "Not given")
                };

                if (!string.IsNullOrWhiteSpace(DriverName))
                {
                    facts.Add(new EmailFact("Driver", DriverName!));
                }

                if (!string.IsNullOrWhiteSpace(CarPlate))
                {
                    facts.Add(new EmailFact("Vehicle", CarPlate!));
                }

                if (!string.IsNullOrWhiteSpace(CompanyName))
                {
                    facts.Add(new EmailFact("Company", CompanyName!));
                }

                facts.Add(new EmailFact("Changed at", OccurredAt.ToString("dd MMM yyyy HH:mm") + " UTC"));

                return new EmailContent
                {
                    Title = Headline,
                    Intro = $"Hello {RecipientName}, there is news about your ride.",
                    Facts = facts,
                    SecurityNote = "Follow the ride in the app under Ride status."
                };
            }
        }
    }
}
