using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Services.Database;

namespace PickMeUp.Services.Orders
{
    public class OrderStateMachine : IOrderStateMachine
    {
        private static readonly Dictionary<OrderTransition, (OrderStatusEnum From, OrderStatusEnum To)> Map = new()
        {
            [OrderTransition.Accept] = (OrderStatusEnum.Pending, OrderStatusEnum.Confirmed),
            [OrderTransition.Pickup] = (OrderStatusEnum.Confirmed, OrderStatusEnum.InProgress),
            [OrderTransition.Complete] = (OrderStatusEnum.InProgress, OrderStatusEnum.Completed)
        };

        private static readonly OrderStatusEnum[] CancellableFrom =
        {
            OrderStatusEnum.Pending,
            OrderStatusEnum.Confirmed,
            OrderStatusEnum.InProgress
        };

        public OrderStatusEnum ResolveTarget(OrderTransition transition, OrderStatusEnum current)
        {
            if (transition == OrderTransition.Cancel)
            {
                if (current == OrderStatusEnum.Completed)
                {
                    throw new BusinessException(
                        "A completed ride cannot be cancelled. Issue a refund through the payment flow instead.");
                }

                if (current == OrderStatusEnum.Cancelled)
                {
                    throw new BusinessException("This order has already been cancelled.");
                }

                if (!CancellableFrom.Contains(current))
                {
                    throw new BusinessException($"An order in state {current} cannot be cancelled.");
                }

                return OrderStatusEnum.Cancelled;
            }

            var (from, to) = Map[transition];

            if (current != from)
            {
                throw new BusinessException(BuildRefusal(transition, current, from));
            }

            return to;
        }

        public void EnsureCancellationReason(string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new BusinessException("A reason is required when cancelling an order.");
            }
        }

        private static string BuildRefusal(OrderTransition transition, OrderStatusEnum current, OrderStatusEnum required) =>
            transition switch
            {
                OrderTransition.Accept when current == OrderStatusEnum.Confirmed =>
                    "This order has already been accepted by a driver.",
                OrderTransition.Accept =>
                    $"Only a pending order can be accepted. This one is {current}.",
                OrderTransition.Pickup =>
                    $"Pickup requires an accepted order. This one is {current}.",
                OrderTransition.Complete =>
                    $"Only a ride in progress can be completed. This one is {current}.",
                _ =>
                    $"An order in state {current} cannot move via {transition}; it must be {required}."
            };
    }
}
