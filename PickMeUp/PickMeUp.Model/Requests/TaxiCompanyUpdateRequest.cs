namespace PickMeUp.Model.Requests
{
    public class TaxiCompanyUpdateRequest
    {
        public string? Name { get; set; }
        public string? HqAddress { get; set; }
        public int? CityId { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? OwnerName { get; set; }
        public decimal? StartingPrice { get; set; }
        public decimal? PricePerKm { get; set; }
        public bool? IsActive { get; set; }
    }
}
