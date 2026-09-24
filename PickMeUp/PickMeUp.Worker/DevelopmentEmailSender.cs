using Microsoft.Extensions.Logging;

namespace PickMeUp.Worker
{
    public class DevelopmentEmailSender : IEmailSender
    {
        private readonly ILogger<DevelopmentEmailSender> _logger;

        public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(
            string toEmail,
            string toName,
            string subject,
            string body,
            string htmlBody,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "SMTP_HOST is not configured, so this message was written to the log instead of being sent.\nTo: {Name} <{Email}>\nSubject: {Subject}\n{Body}",
                toName, toEmail, subject, body);

            return Task.CompletedTask;
        }
    }
}
