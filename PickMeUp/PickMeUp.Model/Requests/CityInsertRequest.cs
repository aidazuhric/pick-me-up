namespace PickMeUp.Model.Requests
{
    public class CityInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
