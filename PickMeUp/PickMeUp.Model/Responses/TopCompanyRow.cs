namespace PickMeUp.Model.Responses
{
    public class TopCompanyRow
    {
        public string CompanyName { get; set; } = string.Empty;

        public string CityName { get; set; } = string.Empty;

        public int CompletedRides { get; set; }

        public decimal Revenue { get; set; }

        public double? AverageRating { get; set; }

        public int ReviewCount { get; set; }
    }
}
