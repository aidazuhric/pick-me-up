using PickMeUp.Common.Services.Configuration;
using PickMeUp.Model.Exceptions;
using Microsoft.Extensions.Options;
using Stripe;

namespace PickMeUp.Services.Payments
{
    public class StripeGateway : IStripeGateway
    {
        private readonly StripeOptions _options;

        public StripeGateway(IOptions<StripeOptions> options)
        {
            _options = options.Value;
            StripeConfiguration.ApiKey = _options.SecretKey;
        }

        public async Task<StripeIntent> CreateIntentAsync(decimal amount, string currency, int orderId, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            var service = new PaymentIntentService();

            try
            {
                var intent = await service.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = ToMinorUnits(amount),
                Currency = currency.ToLowerInvariant(),
                Metadata = new Dictionary<string, string> { ["orderId"] = orderId.ToString() },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
                }, cancellationToken: cancellationToken);

                return new StripeIntent(intent.Id, intent.ClientSecret);
            }
            catch (StripeException exception)
            {
                throw new BusinessException($"Stripe could not start this payment: {exception.StripeError?.Message ?? exception.Message}");
            }
        }

        public async Task<StripeRefund> RefundAsync(string paymentIntentId, decimal amount, string? reason, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            var service = new RefundService();

            try
            {
                var refund = await service.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = ToMinorUnits(amount),
                Reason = RefundReasons.RequestedByCustomer
                }, cancellationToken: cancellationToken);

                return new StripeRefund(refund.Id, FromMinorUnits(refund.Amount));
            }
            catch (StripeException exception)
            {
                throw new BusinessException($"Stripe could not refund this payment: {exception.StripeError?.Message ?? exception.Message}");
            }
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(_options.SecretKey))
            {
                throw new BusinessException("Stripe is not configured. Set STRIPE_SECRET_KEY before taking payments.");
            }
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
                stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, _options.WebhookSecret);
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

        private static long ToMinorUnits(decimal amount) => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

        private static decimal FromMinorUnits(long minor) => minor / 100m;
    }
}
