using PickMeUp.Model.Enums;

namespace PickMeUp.Model.SearchObjects
{
    public class OrdersByPeriodReportSearch : ReportRangeSearch
    {
        public ReportGranularity Granularity { get; set; } = ReportGranularity.Day;
    }
}
