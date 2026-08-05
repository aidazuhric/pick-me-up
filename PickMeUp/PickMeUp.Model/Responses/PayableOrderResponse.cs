namespace PickMeUp.Model.Responses
{
    public class PayableOrderResponse
    {
        public int OrderId { get; set; }
        public string? CompanyName { get; set; }
        public string? DriverName { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string? DestinationAddress { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal Fare { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
