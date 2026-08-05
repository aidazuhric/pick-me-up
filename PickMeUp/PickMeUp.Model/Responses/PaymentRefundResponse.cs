namespace PickMeUp.Model.Responses
{
    public class PaymentRefundResponse
    {
        public decimal RefundedAmount { get; set; }
        public string? Reason { get; set; }
        public DateTime RefundedAt { get; set; }
    }
}
