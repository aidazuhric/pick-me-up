namespace PickMeUp.Model.Responses
{
    public class SupportTicketResponse
    {
        public int Id { get; set; }
        public string SubmitterName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TargetUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedByName { get; set; }
        public List<SupportTicketReplyResponse> Replies { get; set; } = new();
    }
}
