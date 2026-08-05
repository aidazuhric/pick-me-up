namespace PickMeUp.Model.Messaging
{
    public static class MessagingTopology
    {
        public const string Exchange = "pickmeup.events";

        public const string NotificationQueue = "pickmeup.notifications";

        public static class RoutingKeys
        {
            public const string OrderCreated = "order.created";
            public const string OrderStatusChanged = "order.status-changed";
            public const string OrderCancelled = "order.cancelled";
            public const string PaymentConfirmed = "payment.confirmed";
            public const string PaymentRefunded = "payment.refunded";
        }
    }
}
