namespace PickMeUp.Model.Messaging
{
    public class OrderCancelledEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string CancelledBy { get; set; } = string.Empty;

        public override string RoutingKey => MessagingTopology.RoutingKeys.OrderCancelled;

        public override string Subject => $"Ride #{OrderId} was cancelled";

        public override string Body => $"Cancelled by {CancelledBy}. Reason: {Reason}";
    }
}
