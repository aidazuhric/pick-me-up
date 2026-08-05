namespace PickMeUp.Model.Responses
{
    public class SupportTicketReplyResponse
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
