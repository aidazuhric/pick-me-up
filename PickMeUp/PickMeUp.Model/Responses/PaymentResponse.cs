namespace PickMeUp.Model.Responses
{
    public class PaymentResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? DriverName { get; set; }
        public decimal AmountCharged { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public decimal RefundedTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<PaymentRefundResponse> Refunds { get; set; } = new();
    }
}
