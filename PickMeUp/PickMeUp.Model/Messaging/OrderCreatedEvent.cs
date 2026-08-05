namespace PickMeUp.Model.Messaging
{
    public class OrderCreatedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string? DestinationAddress { get; set; }
        public string? CompanyName { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.OrderCreated;

        public override string Subject => $"New ride request #{OrderId}";

        public override string Body =>
            $"A passenger is waiting at {PickupAddress}." +
            (DestinationAddress == null ? string.Empty : $" Destination: {DestinationAddress}.");
    }
}
