namespace PickMeUp.Model.Responses
{
    public class PaymentIntentResponse
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal AmountCharged { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
