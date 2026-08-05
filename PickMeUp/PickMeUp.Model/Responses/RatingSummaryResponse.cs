namespace PickMeUp.Model.Responses
{
    public class RatingSummaryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
