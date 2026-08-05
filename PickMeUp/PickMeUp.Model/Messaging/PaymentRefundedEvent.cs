namespace PickMeUp.Model.Messaging
{
    public class PaymentRefundedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BAM";

        public override string RoutingKey => MessagingTopology.RoutingKeys.PaymentRefunded;

        public override string Subject => $"Refund issued for ride #{OrderId}";

        public override string Body => $"{Amount} {Currency} has been refunded for ride #{OrderId}.";
    }
}
