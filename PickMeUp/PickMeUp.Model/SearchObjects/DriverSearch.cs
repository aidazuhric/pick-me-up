namespace PickMeUp.Model.SearchObjects
{
    public class DriverSearch : BaseSearchObject
    {
        public string? Name { get; set; }
        public string? CompanyName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IncludeResigned { get; set; }
    }
}
