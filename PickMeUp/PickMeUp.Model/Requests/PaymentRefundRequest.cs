namespace PickMeUp.Model.Requests
{
    public class PaymentRefundRequest
    {
        public decimal? Amount { get; set; }
        public string? Reason { get; set; }
        public Guid? IdempotencyKey { get; set; }
    }
}
