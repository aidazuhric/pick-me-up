namespace PickMeUp.Model.Requests
{
    public class SupportTicketInsertRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int? TargetUserId { get; set; }
    }
}
