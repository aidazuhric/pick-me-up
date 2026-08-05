using PickMeUp.Model.Constants;
using PickMeUp.Model.Messaging;
using PickMeUp.Model.Responses;
using PickMeUp.Services.Database;
using PickMeUp.Services.Messaging;
using PickMeUp.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PickMeUp.Services.Orders
{
    public class OrderNotificationDispatcher : IOrderNotificationDispatcher
    {
        private readonly PickMeUpDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly INotificationRecorder _notifications;
        private readonly IShiftService _shiftService;
        private readonly ILogger<OrderNotificationDispatcher> _logger;

        public OrderNotificationDispatcher(
            PickMeUpDbContext dbContext,
            IEventPublisher eventPublisher,
            INotificationRecorder notifications,
            IShiftService shiftService,
            ILogger<OrderNotificationDispatcher> logger)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
            _notifications = notifications;
            _shiftService = shiftService;
            _logger = logger;
        }

        public async Task OrderCreatedAsync(int orderId)
        {
            var order = await LoadAsync(orderId);

            if (order == null)
            {
                return;
            }

            await PublishAsync(new OrderCreatedEvent
            {
                OrderId = order.Id,
                PickupAddress = order.PickupAddress,
                DestinationAddress = order.DestinationAddress,
                CompanyName = order.TaxiCompany?.Name,
                RecipientEmail = order.Passenger.Email,
                RecipientName = FullName(order.Passenger)
            });

            await PushAvailabilityAsync(order, excludeDriverId: null);
        }

        public async Task OrderBecameAvailableAsync(int orderId, int excludeDriverId)
        {
            var order = await LoadAsync(orderId);

            if (order != null)
            {
                await PushAvailabilityAsync(order, excludeDriverId);
            }
        }

        public async Task OrderStatusChangedAsync(int orderId, string fromStatus, string toStatus)
        {
            var order = await LoadAsync(orderId);

            if (order == null)
            {
                return;
            }

            var driverName = order.Driver == null ? null : FullName(order.Driver.User);

            await PublishAsync(new OrderStatusChangedEvent
            {
                OrderId = order.Id,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                DriverName = driverName,
                RecipientEmail = order.Passenger.Email,
                RecipientName = FullName(order.Passenger)
            });

            await DeliverAsync(order.PassengerId, new NotificationMessage
            {
                Title = $"Ride #{order.Id} is now {toStatus}",
                Body = driverName == null
                    ? $"Your ride moved from {fromStatus} to {toStatus}."
                    : $"Your ride moved from {fromStatus} to {toStatus}. Driver: {driverName}.",
                OrderId = order.Id,
                Category = NotificationCategories.OrderStatus
            });
        }

        public async Task OrderCancelledAsync(int orderId, string reason, int cancelledByUserId)
        {
            var order = await LoadAsync(orderId);

            if (order == null)
            {
                return;
            }

            var cancelledByPassenger = cancelledByUserId == order.PassengerId;
            var driverUserId = order.Driver?.UserId;

            var recipientUserId = cancelledByPassenger ? driverUserId : order.PassengerId;

            var recipient = recipientUserId == null
                ? null
                : await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == recipientUserId.Value);

            await PublishAsync(new OrderCancelledEvent
            {
                OrderId = order.Id,
                Reason = reason,
                CancelledBy = cancelledByPassenger ? "the passenger" : "the driver or an administrator",
                RecipientEmail = recipient?.Email ?? order.Passenger.Email,
                RecipientName = recipient == null ? FullName(order.Passenger) : FullName(recipient)
            });

            if (recipientUserId != null)
            {
                await DeliverAsync(recipientUserId.Value, new NotificationMessage
                {
                    Title = $"Ride #{order.Id} was cancelled",
                    Body = $"Reason: {reason}",
                    OrderId = order.Id,
                    Category = NotificationCategories.OrderCancelled
                });
            }
        }

        private async Task PushAvailabilityAsync(Order order, int? excludeDriverId)
        {
            try
            {
                var eligibleDriverIds = _shiftService.QueryDispatchEligibleDriverIds();

                var recipients = await _dbContext.Drivers
                    .Where(d => d.ResignedAt == null)
                    .Where(d => order.TaxiCompanyId == null || d.TaxiCompanyId == order.TaxiCompanyId)
                    .Where(d => excludeDriverId == null || d.Id != excludeDriverId.Value)
                    .Where(d => eligibleDriverIds.Contains(d.Id))
                    .Where(d => !_dbContext.OrderRejections.Any(r => r.OrderId == order.Id && r.DriverId == d.Id))
                    .Select(d => d.UserId)
                    .ToListAsync();

                if (recipients.Count == 0)
                {
                    return;
                }

                await _notifications.DeliverAsync(recipients, new NotificationMessage
                {
                    Title = "New ride available",
                    Body = $"Pick-up at {order.PickupAddress}.",
                    OrderId = order.Id,
                    Category = NotificationCategories.OrderAvailable
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Could not deliver availability for order {OrderId}. The order is unaffected.", order.Id);
            }
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
                    "Could not publish {RoutingKey} for event {EventId}. The transition already succeeded.",
                    integrationEvent.RoutingKey, integrationEvent.EventId);
            }
        }

        private async Task DeliverAsync(int userId, NotificationMessage message)
        {
            try
            {
                await _notifications.DeliverAsync(userId, message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Could not deliver notification for order {OrderId} to user {UserId}. The order is unaffected.",
                    message.OrderId, userId);
            }
        }

        private Task<Order?> LoadAsync(int orderId) =>
            _dbContext.Orders
                .AsNoTracking()
                .Include(o => o.Passenger)
                .Include(o => o.TaxiCompany)
                .Include(o => o.Driver).ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

        private static string FullName(User user) => $"{user.FirstName} {user.LastName}";
    }
}
