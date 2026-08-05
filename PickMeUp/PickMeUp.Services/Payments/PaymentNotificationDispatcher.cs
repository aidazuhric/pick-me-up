using PickMeUp.Model.Messaging;
using PickMeUp.Model.Responses;
using PickMeUp.Services.Database;
using PickMeUp.Services.Messaging;
using PickMeUp.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PickMeUp.Services.Payments
{
    public class PaymentNotificationDispatcher : IPaymentNotificationDispatcher
    {
        private const string PaymentCategory = "payment";

        private readonly PickMeUpDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly ILogger<PaymentNotificationDispatcher> _logger;

        public PaymentNotificationDispatcher(
            PickMeUpDbContext dbContext,
            IEventPublisher eventPublisher,
            INotificationPublisher notificationPublisher,
            ILogger<PaymentNotificationDispatcher> logger)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
            _notificationPublisher = notificationPublisher;
            _logger = logger;
        }

        public async Task PaymentConfirmedAsync(int paymentId)
        {
            var payment = await LoadAsync(paymentId);

            if (payment == null)
            {
                return;
            }

            await PublishAsync(new PaymentConfirmedEvent
            {
                OrderId = payment.OrderId,
                Amount = payment.AmountCharged,
                Currency = payment.Currency,
                RecipientEmail = payment.Order.Passenger.Email,
                RecipientName = FullName(payment.Order.Passenger)
            });

            await PushAsync(payment.Order.PassengerId, new NotificationMessage
            {
                Title = $"Payment received for ride #{payment.OrderId}",
                Body = $"We received {payment.AmountCharged} {payment.Currency}.",
                OrderId = payment.OrderId,
                Category = PaymentCategory
            });
        }

        public async Task PaymentRefundedAsync(int paymentId, decimal refundedAmount)
        {
            var payment = await LoadAsync(paymentId);

            if (payment == null)
            {
                return;
            }

            await PublishAsync(new PaymentRefundedEvent
            {
                OrderId = payment.OrderId,
                Amount = refundedAmount,
                Currency = payment.Currency,
                RecipientEmail = payment.Order.Passenger.Email,
                RecipientName = FullName(payment.Order.Passenger)
            });

            await PushAsync(payment.Order.PassengerId, new NotificationMessage
            {
                Title = $"Refund issued for ride #{payment.OrderId}",
                Body = $"{refundedAmount} {payment.Currency} has been refunded.",
                OrderId = payment.OrderId,
                Category = PaymentCategory
            });
        }

        private async Task PublishAsync(IntegrationEvent integrationEvent)
        {
            try
            {
                await _eventPublisher.PublishAsync(integrationEvent);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Could not publish {RoutingKey} for event {EventId}. The payment already succeeded.",
                    integrationEvent.RoutingKey, integrationEvent.EventId);
            }
        }

        private async Task PushAsync(int userId, NotificationMessage message)
        {
            try
            {
                await _notificationPublisher.PushToUserAsync(userId, message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Could not push payment notification for order {OrderId} to user {UserId}.",
                    message.OrderId, userId);
            }
        }

        private Task<Payment?> LoadAsync(int paymentId) =>
            _dbContext.Payments
                .AsNoTracking()
                .Include(p => p.Order).ThenInclude(o => o.Passenger)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

        private static string FullName(User user) => $"{user.FirstName} {user.LastName}";
    }
}
