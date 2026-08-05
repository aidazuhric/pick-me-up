namespace PickMeUp.Model.Messaging
{
    public class OrderStatusChangedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string FromStatus { get; set; } = string.Empty;
        public string ToStatus { get; set; } = string.Empty;
        public string? DriverName { get; set; }

        public override string RoutingKey => MessagingTopology.RoutingKeys.OrderStatusChanged;

        public override string Subject => $"Ride #{OrderId} is now {ToStatus}";

        public override string Body =>
            $"Your ride moved from {FromStatus} to {ToStatus}." +
            (DriverName == null ? string.Empty : $" Driver: {DriverName}.");
    }
}
