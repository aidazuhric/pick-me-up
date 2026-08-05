using PickMeUp.Model.Constants;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services;
using PickMeUp.WebAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace PickMeUp.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    /// <summary>
    /// Orders placed in the selected period, grouped by day, week or month and by the city of the
    /// company that served the ride. Orders that were never assigned to a company are grouped
    /// separately. Revenue counts the fare of orders that are both Completed and settled by a paid
    /// payment; orders that are still open, cancelled, or awaiting payment contribute to the counts
    /// but not to revenue. Both dates are inclusive; the default period is the last 30 days.
    /// </summary>
    [Authorization(Roles.Admin)]
    [HttpGet("OrdersByPeriod")]
    [Produces("application/pdf")]
    public async Task<IActionResult> OrdersByPeriod([FromQuery] OrdersByPeriodReportSearch search)
    {
        return Pdf(await _service.GenerateOrdersByPeriodAsync(search));
    }

    /// <summary>
    /// The five taxi companies with the most rides completed in the selected period, with the
    /// average rating of the reviews written about the company in the same period. Revenue uses the
    /// same rule as the orders report: the fare of rides that are both Completed and settled by a
    /// paid payment. A ride counts towards the period when it was completed, a review when it was
    /// written. Companies without a completed ride in the period are not ranked. Both dates are
    /// inclusive; the default period is the last 30 days.
    /// </summary>
    [Authorization(Roles.Admin)]
    [HttpGet("TopCompanies")]
    [Produces("application/pdf")]
    public async Task<IActionResult> TopCompanies([FromQuery] ReportRangeSearch search)
    {
        return Pdf(await _service.GenerateTopCompaniesAsync(search));
    }

    /// <summary>
    /// How the calling company's business went in the selected period: rides completed and revenue
    /// broken down per driver and per vehicle, the average rating of the reviews written about the
    /// company and its drivers, and the totals for the period. The company is always taken from the
    /// signed-in account, so a company owner only ever sees its own figures. An administrator has no
    /// company of their own and therefore names one with companyId; that parameter is ignored for
    /// company owners. Revenue counts the fare of rides that are both Completed and settled by a
    /// paid payment. Both dates are inclusive; the default period is the last 30 days.
    /// </summary>
    [Authorization(Roles.Company, Roles.Admin)]
    [HttpGet("CompanyBusiness")]
    [Produces("application/pdf")]
    public async Task<IActionResult> CompanyBusiness([FromQuery] CompanyBusinessReportSearch search)
    {
        return Pdf(await _service.GenerateCompanyBusinessAsync(search));
    }

    private IActionResult Pdf(ReportFile report)
    {
        Response.Headers[HeaderNames.ContentDisposition] =
            new ContentDispositionHeaderValue("inline") { FileName = report.FileName }.ToString();

        return File(report.Content, report.ContentType);
    }
}
