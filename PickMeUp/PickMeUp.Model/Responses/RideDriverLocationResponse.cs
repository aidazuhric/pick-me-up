namespace PickMeUp.Model.Responses
{
    public class RideDriverLocationResponse
    {
        public int OrderId { get; set; }
        public string? DriverName { get; set; }
        public string? CarPlate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
