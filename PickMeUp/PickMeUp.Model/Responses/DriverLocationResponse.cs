namespace PickMeUp.Model.Responses
{
    public class DriverLocationResponse
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CarPlate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsOnShift { get; set; }
        public bool IsOnBreak { get; set; }
        public bool IsDispatchEligible { get; set; }
    }
}
