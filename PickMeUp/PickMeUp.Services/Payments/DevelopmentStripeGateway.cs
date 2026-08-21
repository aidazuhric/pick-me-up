using PickMeUp.Common.Services.Configuration;
using PickMeUp.Model.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace PickMeUp.Services.Payments
{
    /// <summary>
    /// Stands in for Stripe while no test key is configured, so the payment flow can be
    /// exercised locally. Webhook signature verification is still the real Stripe check.
    /// It is registered only in Development and only while STRIPE_SECRET_KEY is empty; supply a
    /// key and the real gateway takes over.
    /// </summary>
    public class DevelopmentStripeGateway : IStripeGateway
    {
        private readonly StripeOptions _options;
        private readonly ILogger<DevelopmentStripeGateway> _logger;

        public DevelopmentStripeGateway(IOptions<StripeOptions> options, ILogger<DevelopmentStripeGateway> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public Task<StripeIntent> CreateIntentAsync(decimal amount, string currency, int orderId, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning(
                "STRIPE_SECRET_KEY is not set: issuing a local stand-in intent for order {OrderId}. No money moves.",
                orderId);

            var id = $"pi_local_{orderId}_{Guid.NewGuid():N}"[..40];

            return Task.FromResult(new StripeIntent(id, $"{id}_secret"));
        }

        public Task<StripeIntent?> FindOpenIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<StripeIntent?>(
                new StripeIntent(paymentIntentId, $"{paymentIntentId}_secret"));
        }

        public Task<StripeRefund> RefundAsync(string paymentIntentId, decimal amount, string? reason, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning(
                "STRIPE_SECRET_KEY is not set: recording a local stand-in refund of {Amount} against {IntentId}.",
                amount, paymentIntentId);

            return Task.FromResult(new StripeRefund($"re_local_{Guid.NewGuid():N}"[..40], amount));
        }

        public StripeWebhookEvent ReadWebhook(string payload, string signatureHeader)
        {
            if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
            {
                throw new BusinessException("Stripe webhook secret is not configured.");
            }

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    signatureHeader,
                    _options.WebhookSecret,
                    throwOnApiVersionMismatch: false);
            }
            catch (StripeException exception)
            {
                throw new BusinessException($"The webhook signature could not be verified: {exception.Message}");
            }
            catch (Exception exception)
            {
                throw new BusinessException($"The webhook payload could not be read: {exception.Message}");
            }

            var intentId = stripeEvent.Data.Object is PaymentIntent intent ? intent.Id : null;

            return new StripeWebhookEvent(stripeEvent.Type, intentId);
        }
    }
}
