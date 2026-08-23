namespace PickMeUp.Model.Responses
{
    public class TaxiCompanyDetailsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string HqAddress { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public decimal PricePerKm { get; set; }
        public string? LogoImageUrl { get; set; }
        public int VehicleCount { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
