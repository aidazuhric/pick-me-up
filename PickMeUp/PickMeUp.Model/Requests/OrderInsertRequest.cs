namespace PickMeUp.Model.Requests
{
    public class OrderInsertRequest
    {
        public int? TaxiCompanyId { get; set; }
        public double PickupLat { get; set; }
        public double PickupLng { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public double? DestinationLat { get; set; }
        public double? DestinationLng { get; set; }
        public string? DestinationAddress { get; set; }
        public decimal? DistanceKm { get; set; }
    }
}
