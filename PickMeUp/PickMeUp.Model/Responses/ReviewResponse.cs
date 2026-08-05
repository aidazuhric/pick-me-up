namespace PickMeUp.Model.Responses
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? DriverName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
