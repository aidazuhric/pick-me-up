using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface IPaymentService : IBaseReadService<PaymentResponse, PaymentSearch>
    {
        Task<PaymentIntentResponse> CreateIntentAsync(PaymentIntentRequest request);

        Task<bool> HandleWebhookAsync(string payload, string signatureHeader);

        Task<PaymentResponse> RefundAsync(int id, PaymentRefundRequest request);

        Task<bool> RefundCancelledOrderAsync(int orderId, string reason);

        Task<PaymentResponse> GetByOrderAsync(int orderId);

        Task<List<PayableOrderResponse>> GetPayableOrdersAsync();

        PaymentConfigResponse GetConfiguration();
    }
}
