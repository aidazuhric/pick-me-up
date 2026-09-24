using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface IOrderService : IBaseReadService<OrderResponse, OrderSearch>
    {
        Task<OrderResponse> CreateAsync(OrderInsertRequest request);

        Task<OrderQuoteResponse> QuoteAsync(OrderInsertRequest request);

        Task<OrderResponse> AcceptAsync(int id, OrderAcceptRequest request);

        Task<OrderResponse> RejectAsync(int id);

        Task<OrderResponse> PickupAsync(int id);

        Task<OrderResponse> CompleteAsync(int id);

        Task<OrderResponse> CancelAsync(int id, OrderCancelRequest request);

        Task<bool> DriverHasActiveOrderAsync(int driverId);
    }
}
