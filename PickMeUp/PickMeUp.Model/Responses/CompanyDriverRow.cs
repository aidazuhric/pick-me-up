namespace PickMeUp.Model.Responses
{
    public class CompanyDriverRow
    {
        public string DriverName { get; set; } = string.Empty;

        public int CompletedRides { get; set; }

        public decimal Revenue { get; set; }

        public double? AverageRating { get; set; }

        public int ReviewCount { get; set; }
    }
}
