using PickMeUp.Model.Enums;

namespace PickMeUp.Model.SearchObjects
{
    public class ShiftSearch : BaseSearchObject
    {
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string? CompanyName { get; set; }
        public bool? OngoingOnly { get; set; }
        public ShiftPeriod? Period { get; set; }
        public DateTime? StartedFrom { get; set; }
        public DateTime? StartedTo { get; set; }
    }
}
