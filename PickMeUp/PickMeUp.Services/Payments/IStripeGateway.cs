namespace PickMeUp.Services.Payments
{
    public interface IStripeGateway
    {
        Task<StripeIntent> CreateIntentAsync(decimal amount, string currency, int orderId, CancellationToken cancellationToken = default);

        Task<StripeIntentState?> ReadIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);

        Task<StripeRefund> RefundAsync(string paymentIntentId, decimal amount, string? reason, CancellationToken cancellationToken = default);

        StripeWebhookEvent ReadWebhook(string payload, string signatureHeader);
    }

    public record StripeIntent(string PaymentIntentId, string ClientSecret);

    public record StripeIntentState(string PaymentIntentId, string ClientSecret, string Status)
    {
        public bool IsSucceeded => Status == "succeeded";

        public bool IsReusable => Status != "succeeded" && Status != "canceled";
    }

    public record StripeRefund(string RefundId, decimal Amount);

    public record StripeWebhookEvent(string Type, string? PaymentIntentId, int? OrderId);
}
