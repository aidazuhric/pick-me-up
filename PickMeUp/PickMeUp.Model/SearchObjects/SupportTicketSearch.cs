using PickMeUp.Model.Enums;

namespace PickMeUp.Model.SearchObjects
{
    public class SupportTicketSearch : BaseSearchObject
    {
        public SupportTicketStatusEnum? Status { get; set; }
        public string? Subject { get; set; }
        public string? SubmitterName { get; set; }
        public int? TargetUserId { get; set; }
        public ShiftPeriod? Period { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
