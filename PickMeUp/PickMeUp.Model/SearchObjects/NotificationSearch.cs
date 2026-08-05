using PickMeUp.Model.Enums;

namespace PickMeUp.Model.SearchObjects
{
    public class NotificationSearch : BaseSearchObject
    {
        public bool? IsRead { get; set; }
        public string? Category { get; set; }
        public int? OrderId { get; set; }
        public ShiftPeriod? Period { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
