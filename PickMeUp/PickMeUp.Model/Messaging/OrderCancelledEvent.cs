using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public class OrderCancelledEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string CancelledBy { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public string? DestinationAddress { get; set; }
        public bool WasPaid { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.OrderCancelled;

        public override string Subject => $"Ride #{OrderId} was cancelled";

        protected override EmailContent Content => new()
        {
            Title = "The ride was cancelled",
            Intro = $"Hello {RecipientName}, ride #{OrderId} will not go ahead.",
            Facts = new List<EmailFact>
            {
                new("Ride number", $"#{OrderId}"),
                new("From", PickupAddress),
                new("To", DestinationAddress ?? "Not given"),
                new("Cancelled by", CancelledBy),
                new("Reason", Reason),
                new("Cancelled at", OccurredAt.ToString("dd MMM yyyy HH:mm") + " UTC")
            },
            Highlight = WasPaid
                ? "The amount you paid is on its way back to your card."
                : null,
            SecurityNote = "You can book another ride from the app whenever you are ready."
        };
    }
}
