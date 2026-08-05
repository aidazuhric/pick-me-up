using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface IReportService
    {
        Task<ReportFile> GenerateOrdersByPeriodAsync(OrdersByPeriodReportSearch search);

        Task<ReportFile> GenerateTopCompaniesAsync(ReportRangeSearch search);
    }
}
