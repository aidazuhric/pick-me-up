namespace PickMeUp.Model.SearchObjects
{
    public class TaxiCompanySearch : BaseSearchObject
    {
        public string? Name { get; set; }

        public int? CityId { get; set; }

        public string? CityName { get; set; }

        public bool? IsActive { get; set; }
    }
}
