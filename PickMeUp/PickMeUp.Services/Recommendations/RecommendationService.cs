using System.Globalization;
using PickMeUp.Model.Constants;
using PickMeUp.Model.Enums;
using PickMeUp.Model.Exceptions;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace PickMeUp.Services.Recommendations
{
    public class RecommendationService : IRecommendationService
    {
        private readonly PickMeUpDbContext _dbContext;
        private readonly IAuthenticatedUserAccessor _userAccessor;

        public RecommendationService(PickMeUpDbContext dbContext, IAuthenticatedUserAccessor userAccessor)
        {
            _dbContext = dbContext;
            _userAccessor = userAccessor;
        }

        public async Task<PageResult<RecommendationResponse>> GetForCurrentPassengerAsync(
            RecommendationSearch? search = null)
        {
            var userId = _userAccessor.GetUserId()
                ?? throw new BusinessException("The caller could not be identified.");

            var ordered = (int)InteractionSignalType.Ordered;
            var completed = (int)InteractionSignalType.Completed;
            var reviewed = (int)InteractionSignalType.Reviewed;

            var popularity = _dbContext.InteractionHistories
                .AsNoTracking()
                .GroupBy(i => i.TaxiCompanyId)
                .Select(g => new
                {
                    TaxiCompanyId = g.Key,
                    CompletedRides = g.Sum(i => i.SignalTypeId == completed ? 1 : 0),
                    ReviewCount = g.Sum(i => i.SignalTypeId == reviewed ? 1 : 0),
                    RatingTotal = g.Sum(i => i.SignalTypeId == reviewed ? i.Value : 0m)
                });

            var registeredCityId = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.CityId)
                .FirstOrDefaultAsync();

            var cityId = registeredCityId ?? search?.CityId;

            var candidates = await (from company in _dbContext.TaxiCompanies.AsNoTracking()
                                    where company.IsActive
                                       && (cityId == null || company.CityId == cityId)
                                    join signals in popularity
                                        on company.Id equals signals.TaxiCompanyId into matches
                                    from signals in matches.DefaultIfEmpty()
                                    select new CompanyPopularity
                                    {
                                        TaxiCompanyId = company.Id,
                                        CompanyName = company.Name,
                                        CityId = company.CityId,
                                        CityName = company.City.Name,
                                        CompletedRides = (int?)signals.CompletedRides ?? 0,
                                        ReviewCount = (int?)signals.ReviewCount ?? 0,
                                        RatingTotal = (decimal?)signals.RatingTotal
                                    })
                                   .ToListAsync();

            var personal = await _dbContext.InteractionHistories
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .GroupBy(i => i.TaxiCompanyId)
                .Select(g => new PersonalSignals
                {
                    TaxiCompanyId = g.Key,
                    Orders = g.Sum(i => i.SignalTypeId == ordered ? 1 : 0),
                    CompletedRides = g.Sum(i => i.SignalTypeId == completed ? 1 : 0),
                    ReviewCount = g.Sum(i => i.SignalTypeId == reviewed ? 1 : 0),
                    RatingTotal = g.Sum(i => i.SignalTypeId == reviewed ? i.Value : 0m)
                })
                .ToListAsync();

            var byCompany = personal.ToDictionary(p => p.TaxiCompanyId);
            var homeCityId = ResolveHomeCityId(candidates, byCompany);
            var maxCompletedRides = candidates.Count == 0 ? 0 : candidates.Max(c => c.CompletedRides);

            var ranked = candidates
                .Select(candidate => Score(
                    candidate,
                    byCompany.TryGetValue(candidate.TaxiCompanyId, out var mine) ? mine : null,
                    maxCompletedRides,
                    homeCityId,
                    byCompany.Count > 0))
                .OrderByDescending(r => r.Score)
                .ThenByDescending(r => r.CompletedRidesOverall)
                .ThenBy(r => r.CompanyName)
                .ToList();

            var page = Math.Max(search?.Page ?? 1, 1);
            var pageSize = Math.Clamp(
                search?.PageSize ?? RecommendationSearch.DefaultRecommendationCount,
                1,
                BaseSearchObject.MaxPageSize);

            return new PageResult<RecommendationResponse>
            {
                Items = ranked.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = ranked.Count
            };
        }

        private static int? ResolveHomeCityId(
            List<CompanyPopularity> candidates, Dictionary<int, PersonalSignals> personal)
        {
            if (personal.Count == 0)
            {
                return null;
            }

            return candidates
                .Where(candidate => personal.ContainsKey(candidate.TaxiCompanyId))
                .GroupBy(candidate => candidate.CityId)
                .Select(g => new
                {
                    CityId = g.Key,
                    Rides = g.Sum(candidate => personal[candidate.TaxiCompanyId].Orders
                        + personal[candidate.TaxiCompanyId].CompletedRides)
                })
                .OrderByDescending(g => g.Rides)
                .ThenBy(g => g.CityId)
                .Select(g => (int?)g.CityId)
                .FirstOrDefault();
        }

        private static RecommendationResponse Score(
            CompanyPopularity candidate,
            PersonalSignals? mine,
            int maxCompletedRides,
            int? homeCityId,
            bool hasHistory)
        {
            var ownRating = Average(mine?.RatingTotal, mine?.ReviewCount ?? 0);
            var overallRating = Average(candidate.RatingTotal, candidate.ReviewCount);

            var usage = Saturated(mine?.CompletedRides ?? 0);
            var loyalty = Saturated(mine?.Orders ?? 0);
            var personalRating = Normalized(ownRating);

            var contentScore =
                RecommenderWeights.ContentCompletedRides * usage
                + RecommenderWeights.ContentOrders * loyalty
                + RecommenderWeights.ContentOwnRating * personalRating;

            var ridesShare = maxCompletedRides == 0
                ? 0d
                : (double)candidate.CompletedRides / maxCompletedRides;

            var popularityScore =
                RecommenderWeights.PopularityCompletedRides * ridesShare
                + RecommenderWeights.PopularityRating * Normalized(overallRating);

            var isInHomeCity = homeCityId != null && candidate.CityId == homeCityId.Value;

            var score =
                RecommenderWeights.Content * contentScore
                + RecommenderWeights.Popularity * popularityScore
                + (isInHomeCity ? RecommenderWeights.HomeCityBonus : 0d);

            return new RecommendationResponse
            {
                TaxiCompanyId = candidate.TaxiCompanyId,
                CompanyName = candidate.CompanyName,
                CityName = candidate.CityName,
                Score = Math.Round(score, 4),
                ContentScore = Math.Round(contentScore, 4),
                PopularityScore = Math.Round(popularityScore, 4),
                IsInHomeCity = isInHomeCity,
                YourOrders = mine?.Orders ?? 0,
                YourCompletedRides = mine?.CompletedRides ?? 0,
                YourAverageRating = ownRating == null ? null : Math.Round(ownRating.Value, 2),
                CompletedRidesOverall = candidate.CompletedRides,
                AverageRatingOverall = overallRating == null ? null : Math.Round(overallRating.Value, 2),
                ReviewCountOverall = candidate.ReviewCount,
                Explanation = Explain(candidate, mine, ownRating, overallRating, isInHomeCity, hasHistory)
            };
        }

        private static string Explain(
            CompanyPopularity candidate,
            PersonalSignals? mine,
            double? ownRating,
            double? overallRating,
            bool isInHomeCity,
            bool hasHistory)
        {
            if (mine != null && mine.CompletedRides > 0 && ownRating != null)
            {
                return $"You completed {Rides(mine.CompletedRides)} with them and rated them "
                    + $"{Format(ownRating.Value)} out of 5.";
            }

            if (mine != null && mine.CompletedRides > 0)
            {
                return $"You completed {Rides(mine.CompletedRides)} with them.";
            }

            if (mine != null && mine.Orders > 0)
            {
                return $"You have ordered from them {Times(mine.Orders)} before.";
            }

            if (isInHomeCity && overallRating != null)
            {
                return $"Rated {Format(overallRating.Value)} out of 5 by other passengers in {candidate.CityName}, "
                    + $"where you ride most.";
            }

            if (isInHomeCity)
            {
                return $"A company in {candidate.CityName}, where you ride most, that you have not tried yet.";
            }

            if (!hasHistory && overallRating != null)
            {
                return $"Popular with other passengers: {Rides(candidate.CompletedRides)} completed and "
                    + $"rated {Format(overallRating.Value)} out of 5.";
            }

            if (!hasHistory)
            {
                return $"Popular with other passengers: {Rides(candidate.CompletedRides)} completed so far.";
            }

            if (overallRating != null)
            {
                return $"New to you, but rated {Format(overallRating.Value)} out of 5 across "
                    + $"{Rides(candidate.CompletedRides)}.";
            }

            return "New to you and still waiting for its first reviews.";
        }

        private static double Saturated(int count) =>
            Math.Min((double)count / RecommenderWeights.UsageSaturation, 1d);

        private static double Normalized(double? rating) =>
            rating == null ? RecommenderWeights.NeutralRating : (rating.Value - 1d) / 4d;

        private static double? Average(decimal? total, int count) =>
            count == 0 || total == null ? null : (double)total.Value / count;

        private static string Format(double value) =>
            Math.Round(value, 1).ToString("0.#", CultureInfo.InvariantCulture);

        private static string Rides(int count) => count == 1 ? "1 ride" : $"{count} rides";

        private static string Times(int count) => count == 1 ? "once" : $"{count} times";

        private sealed class CompanyPopularity
        {
            public int TaxiCompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;
            public int CityId { get; set; }
            public string CityName { get; set; } = string.Empty;
            public int CompletedRides { get; set; }
            public int ReviewCount { get; set; }
            public decimal? RatingTotal { get; set; }
        }

        private sealed class PersonalSignals
        {
            public int TaxiCompanyId { get; set; }
            public int Orders { get; set; }
            public int CompletedRides { get; set; }
            public int ReviewCount { get; set; }
            public decimal RatingTotal { get; set; }
        }
    }
}
