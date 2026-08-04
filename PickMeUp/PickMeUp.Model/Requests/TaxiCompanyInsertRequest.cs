namespace PickMeUp.Model.Requests
{
    public class TaxiCompanyInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string HqAddress { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public decimal PricePerKm { get; set; }
        public bool IsActive { get; set; } = true;

        public string LoginUsername { get; set; } = string.Empty;
        public string? LoginEmail { get; set; }
        public string? LoginPassword { get; set; }
    }
}
