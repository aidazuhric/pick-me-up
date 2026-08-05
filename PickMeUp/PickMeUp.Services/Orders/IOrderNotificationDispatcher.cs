namespace PickMeUp.Services.Orders
{
    public interface IOrderNotificationDispatcher
    {
        Task OrderCreatedAsync(int orderId);

        Task OrderBecameAvailableAsync(int orderId, int excludeDriverId);

        Task OrderStatusChangedAsync(int orderId, string fromStatus, string toStatus);

        Task OrderCancelledAsync(int orderId, string reason, int cancelledByUserId);
    }
}
