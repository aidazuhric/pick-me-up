namespace PickMeUp.Services.Payments
{
    public interface IPaymentNotificationDispatcher
    {
        Task PaymentConfirmedAsync(int paymentId);

        Task PaymentRefundedAsync(int paymentId, decimal refundedAmount);
    }
}
