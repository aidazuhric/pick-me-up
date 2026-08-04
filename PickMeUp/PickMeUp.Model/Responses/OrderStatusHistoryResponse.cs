namespace PickMeUp.Model.Responses
{
    public class OrderStatusHistoryResponse
    {
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
    }
}
