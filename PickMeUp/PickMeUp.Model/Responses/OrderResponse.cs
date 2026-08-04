namespace PickMeUp.Model.Responses
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? DriverName { get; set; }
        public string? CarPlate { get; set; }
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public double? DestinationLat { get; set; }
        public double? DestinationLng { get; set; }
        public string? DestinationAddress { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal? Fare { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsGeneralOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public List<OrderStatusHistoryResponse> StatusHistory { get; set; } = new();
    }
}
