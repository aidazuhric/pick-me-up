using PickMeUp.Model.Enums;

namespace PickMeUp.Model.SearchObjects
{
    public class ReviewSearch : BaseSearchObject
    {
        public int? OrderId { get; set; }
        public int? TargetCompanyId { get; set; }
        public int? TargetDriverId { get; set; }
        public int? Rating { get; set; }
        public int? MinRating { get; set; }
        public string? AuthorName { get; set; }
        public ShiftPeriod? Period { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
