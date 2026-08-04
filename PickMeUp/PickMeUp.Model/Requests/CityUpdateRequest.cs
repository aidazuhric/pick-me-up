namespace PickMeUp.Model.Requests
{
    public class CityUpdateRequest
    {
        public string? Name { get; set; }
        public string? PostalCode { get; set; }
        public bool? IsActive { get; set; }
    }
}
