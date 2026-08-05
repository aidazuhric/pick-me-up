namespace PickMeUp.Services.Reports
{
    public interface IReportDocumentBuilder
    {
        byte[] Render(ReportDefinition definition);
    }
}
