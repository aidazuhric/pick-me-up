namespace PickMeUp.Model.Responses
{
    public class OrderQuoteResponse
    {
        public decimal? DistanceKm { get; set; }
        public decimal Fare { get; set; }
        public string Currency { get; set; } = "BAM";
        public bool IsGeneralOrder { get; set; }
        public string? CompanyName { get; set; }
    }
}
