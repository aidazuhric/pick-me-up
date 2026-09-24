using PickMeUp.Model.Emails;

namespace PickMeUp.Model.Messaging
{
    public abstract class IntegrationEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        public string RecipientEmail { get; set; } = string.Empty;

        public string RecipientName { get; set; } = string.Empty;

        public abstract string RoutingKey { get; }

        public abstract string Subject { get; }

        protected abstract EmailContent Content { get; }

        public string Body => EmailTemplate.RenderText(Content);

        public string HtmlBody => EmailTemplate.RenderHtml(Subject, Content);
    }
}
