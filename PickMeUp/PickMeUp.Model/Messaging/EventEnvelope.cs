namespace PickMeUp.Model.Messaging
{
    public class EventEnvelope
    {
        public string RoutingKey { get; set; } = string.Empty;
        public Guid EventId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = string.Empty;
    }
}
