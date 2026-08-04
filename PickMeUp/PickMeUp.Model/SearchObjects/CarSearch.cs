namespace PickMeUp.Model.SearchObjects
{
    public class CarSearch : BaseSearchObject
    {
        public string? Plate { get; set; }
        public string? Model { get; set; }
        public int? VehicleCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool? IsActive { get; set; }
    }
}
