namespace PickMeUp.Model.Responses
{
    public class RecommendationResponse
    {
        public int TaxiCompanyId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string CityName { get; set; } = string.Empty;

        public double Score { get; set; }

        public double ContentScore { get; set; }

        public double PopularityScore { get; set; }

        public bool IsInHomeCity { get; set; }

        public string Explanation { get; set; } = string.Empty;

        public int YourOrders { get; set; }

        public int YourCompletedRides { get; set; }

        public double? YourAverageRating { get; set; }

        public int CompletedRidesOverall { get; set; }

        public double? AverageRatingOverall { get; set; }

        public int ReviewCountOverall { get; set; }
    }
}
