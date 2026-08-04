namespace PickMeUp.Model.SearchObjects
{
    public class CarAssignmentSearch : BaseSearchObject
    {
        public int? CarId { get; set; }
        public int? DriverId { get; set; }
        public string? Plate { get; set; }
        public string? DriverName { get; set; }
        public bool? IsActive { get; set; }
        public bool? OngoingOnly { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
    }
}
