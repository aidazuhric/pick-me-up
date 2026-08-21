namespace PickMeUp.Services.Payments
{
    public interface IStripeGateway
    {
        Task<StripeIntent> CreateIntentAsync(decimal amount, string currency, int orderId, CancellationToken cancellationToken = default);

        Task<StripeIntent?> FindOpenIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);

        Task<StripeRefund> RefundAsync(string paymentIntentId, decimal amount, string? reason, CancellationToken cancellationToken = default);

        StripeWebhookEvent ReadWebhook(string payload, string signatureHeader);
    }

    public record StripeIntent(string PaymentIntentId, string ClientSecret);

    public record StripeRefund(string RefundId, decimal Amount);

    public record StripeWebhookEvent(string Type, string? PaymentIntentId);
}
