namespace PickMeUp.Model.Messaging
{
    public class PaymentConfirmedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BAM";

        public override string RoutingKey => MessagingTopology.RoutingKeys.PaymentConfirmed;

        public override string Subject => $"Payment received for ride #{OrderId}";

        public override string Body => $"We received {Amount} {Currency} for ride #{OrderId}.";
    }
}
