using PickMeUp.Model.Enums;

namespace PickMeUp.Services.Orders
{
    public interface IOrderStateMachine
    {
        OrderStatusEnum ResolveTarget(OrderTransition transition, OrderStatusEnum current);

        void EnsureCancellationReason(string? reason);
    }
}
