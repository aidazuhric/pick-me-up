namespace PickMeUp.Model.Responses
{
    public class NotificationMessage
    {
        public int? NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        public int? OrderId { get; set; }
        public string? Category { get; set; }
    }
}
