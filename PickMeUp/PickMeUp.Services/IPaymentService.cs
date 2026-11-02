using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

using PickMeUp.Model.Enums;

namespace PickMeUp.Services
{
    public interface IPaymentService : IBaseReadService<PaymentResponse, PaymentSearch>
    {
        Task<PaymentIntentResponse> CreateIntentAsync(PaymentIntentRequest request);

        Task<bool> HandleWebhookAsync(string payload, string signatureHeader);

        Task<PaymentResponse> RefundAsync(int id, PaymentRefundRequest request);

        Task<RefundOutcome> RefundCancelledOrderAsync(int orderId, string reason);

        Task<PaymentResponse> GetByOrderAsync(int orderId);

        Task<List<PayableOrderResponse>> GetPayableOrdersAsync();

        PaymentConfigResponse GetConfiguration();
    }
}
