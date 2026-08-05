using System.Globalization;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using PickMeUp.Services.Reports;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services
{
    public class ReportService : IReportService
    {
        private const string UnassignedCity = "No company assigned";
        private const int DefaultRangeDays = 30;
        private const int MaxRangeDays = 366;
        private const int TopCompanyCount = 5;

        protected readonly PickMeUpDbContext _dbContext;
        protected readonly IReportDocumentBuilder _documentBuilder;

        public ReportService(PickMeUpDbContext dbContext, IReportDocumentBuilder documentBuilder)
        {
            _dbContext = dbContext;
            _documentBuilder = documentBuilder;
        }

        public async Task<ReportFile> GenerateOrdersByPeriodAsync(OrdersByPeriodReportSearch search)
        {
            var range = ReportRange.Of(search);
            var field = DateTruncField(search.Granularity);

            var completed = (int)OrderStatusEnum.Completed;
            var cancelled = (int)OrderStatusEnum.Cancelled;

            var bucketed = _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= range.From && o.CreatedAt < range.ToExclusive)
                .Select(o => new
                {
                    Bucket = PostgresFunctions.DateTrunc(field, o.CreatedAt),
                    CityId = _dbContext.TaxiCompanies
                        .Where(t => t.Id == o.TaxiCompanyId)
                        .Select(t => (int?)t.CityId)
                        .FirstOrDefault(),
                    o.OrderStatusId,
                    Fare = o.Fare ?? 0m,
                    IsPaid = _dbContext.Payments.Any(p => p.OrderId == o.Id && p.IsPaid)
                });

            var grouped = bucketed
                .GroupBy(x => new { x.Bucket, x.CityId })
                .Select(g => new
                {
                    g.Key.Bucket,
                    g.Key.CityId,
                    OrderCount = g.Count(),
                    CancelledCount = g.Sum(x => x.OrderStatusId == cancelled ? 1 : 0),
                    Revenue = g.Sum(x => x.OrderStatusId == completed && x.IsPaid ? x.Fare : 0m)
                });

            var rows = await (from row in grouped
                              join city in _dbContext.Cities on row.CityId equals (int?)city.Id into matches
                              from city in matches.DefaultIfEmpty()
                              orderby row.Bucket, city.Name
                              select new OrdersByPeriodRow
                              {
                                  PeriodStart = row.Bucket,
                                  CityName = city == null ? UnassignedCity : city.Name,
                                  OrderCount = row.OrderCount,
                                  CancelledCount = row.CancelledCount,
                                  Revenue = row.Revenue
                              })
                             .ToListAsync();

            var definition = new ReportDefinition
            {
                Title = "Orders by period and city",
                PeriodLabel = $"{range.Label} · grouped by {search.Granularity.ToString().ToLowerInvariant()}",
                Summary = new List<string>
                {
                    $"Orders placed: {rows.Sum(r => r.OrderCount)}",
                    $"Cancelled: {rows.Sum(r => r.CancelledCount)}",
                    $"Revenue (completed and paid rides): {Money(rows.Sum(r => r.Revenue))}"
                },
                Tables = new List<ReportTable>
                {
                    new ReportTable
                    {
                        Caption = "Breakdown",
                        Columns = new List<ReportColumn>
                        {
                            new ReportColumn("Period", 2),
                            new ReportColumn("City", 3),
                            new ReportColumn("Orders", 2, alignRight: true),
                            new ReportColumn("Cancelled", 2, alignRight: true),
                            new ReportColumn("Revenue", 2, alignRight: true)
                        },
                        Rows = rows.Select(r => new List<string>
                        {
                            BucketLabel(r.PeriodStart, search.Granularity),
                            r.CityName,
                            r.OrderCount.ToString(CultureInfo.InvariantCulture),
                            r.CancelledCount.ToString(CultureInfo.InvariantCulture),
                            Money(r.Revenue)
                        }).ToList(),
                        EmptyMessage = "No orders were placed in the selected period."
                    }
                }
            };

            return new ReportFile(
                $"orders-by-period-{range.FileStamp}.pdf",
                _documentBuilder.Render(definition));
        }

        public async Task<ReportFile> GenerateTopCompaniesAsync(ReportRangeSearch search)
        {
            var range = ReportRange.Of(search);
            var completed = (int)OrderStatusEnum.Completed;

            var rides = _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.TaxiCompanyId != null
                    && o.OrderStatusId == completed
                    && o.CompletedAt >= range.From
                    && o.CompletedAt < range.ToExclusive)
                .Select(o => new
                {
                    CompanyId = o.TaxiCompanyId!.Value,
                    Fare = o.Fare ?? 0m,
                    IsPaid = _dbContext.Payments.Any(p => p.OrderId == o.Id && p.IsPaid)
                })
                .GroupBy(x => x.CompanyId)
                .Select(g => new
                {
                    CompanyId = g.Key,
                    CompletedRides = g.Count(),
                    Revenue = g.Sum(x => x.IsPaid ? x.Fare : 0m)
                });

            var ratings = _dbContext.Reviews
                .AsNoTracking()
                .Where(r => r.TargetCompanyId != null
                    && r.CreatedAt >= range.From
                    && r.CreatedAt < range.ToExclusive)
                .GroupBy(r => r.TargetCompanyId!.Value)
                .Select(g => new
                {
                    CompanyId = g.Key,
                    Average = g.Average(r => (double)r.Rating),
                    ReviewCount = g.Count()
                });

            var rows = await (from ride in rides
                              join company in _dbContext.TaxiCompanies on ride.CompanyId equals company.Id
                              join rating in ratings on ride.CompanyId equals rating.CompanyId into rated
                              from rating in rated.DefaultIfEmpty()
                              orderby ride.CompletedRides descending, ride.Revenue descending
                              select new TopCompanyRow
                              {
                                  CompanyName = company.Name,
                                  CityName = company.City.Name,
                                  CompletedRides = ride.CompletedRides,
                                  Revenue = ride.Revenue,
                                  AverageRating = rating == null ? null : (double?)rating.Average,
                                  ReviewCount = rating == null ? 0 : rating.ReviewCount
                              })
                             .Take(TopCompanyCount)
                             .ToListAsync();

            var definition = new ReportDefinition
            {
                Title = $"Top {TopCompanyCount} taxi companies",
                PeriodLabel = range.Label,
                Summary = new List<string>
                {
                    $"Companies with at least one completed ride: {rows.Count}",
                    $"Completed rides in the top {TopCompanyCount}: {rows.Sum(r => r.CompletedRides)}",
                    $"Revenue in the top {TopCompanyCount}: {Money(rows.Sum(r => r.Revenue))}"
                },
                Tables = new List<ReportTable>
                {
                    new ReportTable
                    {
                        Caption = "Ranked by completed rides, then revenue",
                        Columns = new List<ReportColumn>
                        {
                            new ReportColumn("#", 1),
                            new ReportColumn("Company", 4),
                            new ReportColumn("City", 3),
                            new ReportColumn("Completed rides", 2, alignRight: true),
                            new ReportColumn("Revenue", 2, alignRight: true),
                            new ReportColumn("Average rating", 2, alignRight: true)
                        },
                        Rows = rows.Select((r, index) => new List<string>
                        {
                            (index + 1).ToString(CultureInfo.InvariantCulture),
                            r.CompanyName,
                            r.CityName,
                            r.CompletedRides.ToString(CultureInfo.InvariantCulture),
                            Money(r.Revenue),
                            Rating(r.AverageRating, r.ReviewCount)
                        }).ToList(),
                        EmptyMessage = "No rides were completed in the selected period."
                    }
                }
            };

            return new ReportFile(
                $"top-companies-{range.FileStamp}.pdf",
                _documentBuilder.Render(definition));
        }

        protected static string Money(decimal amount) =>
            amount.ToString("N2", CultureInfo.InvariantCulture) + " BAM";

        protected static string Rating(double? average, int reviewCount) =>
            average == null
                ? "no reviews"
                : $"{Math.Round(average.Value, 2).ToString("N2", CultureInfo.InvariantCulture)} ({reviewCount})";

        private static string DateTruncField(ReportGranularity granularity) => granularity switch
        {
            ReportGranularity.Week => "week",
            ReportGranularity.Month => "month",
            _ => "day"
        };

        private static string BucketLabel(DateTime bucket, ReportGranularity granularity) => granularity switch
        {
            ReportGranularity.Week => $"week of {bucket:yyyy-MM-dd}",
            ReportGranularity.Month => bucket.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            _ => bucket.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

        protected sealed class ReportRange
        {
            private ReportRange(DateTime from, DateTime toInclusive)
            {
                From = from;
                ToInclusive = toInclusive;
                ToExclusive = toInclusive.AddDays(1);
            }

            public DateTime From { get; }

            public DateTime ToInclusive { get; }

            public DateTime ToExclusive { get; }

            public string Label =>
                $"{From:yyyy-MM-dd} to {ToInclusive:yyyy-MM-dd}";

            public string FileStamp =>
                $"{From:yyyyMMdd}-{ToInclusive:yyyyMMdd}";

            public static ReportRange Of(ReportRangeSearch search)
            {
                var toInclusive = AsUtcDate(search.To) ?? DateTime.UtcNow.Date;
                var from = AsUtcDate(search.From) ?? toInclusive.AddDays(-DefaultRangeDays);

                if (from > toInclusive)
                {
                    throw new BusinessException("The start of the period must not be after its end.");
                }

                if ((toInclusive - from).TotalDays > MaxRangeDays)
                {
                    throw new BusinessException($"A report can cover at most {MaxRangeDays} days.");
                }

                return new ReportRange(from, toInclusive);
            }

            private static DateTime? AsUtcDate(DateTime? value) =>
                value == null
                    ? null
                    : DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc);
        }
    }
}
