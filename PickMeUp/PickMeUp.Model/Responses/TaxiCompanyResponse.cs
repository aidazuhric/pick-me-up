namespace PickMeUp.Model.Responses
{
    public class TaxiCompanyResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HqAddress { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public decimal PricePerKm { get; set; }
        public string? LogoImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? InitialLoginUsername { get; set; }
        public string? InitialLoginPassword { get; set; }
    }
}
