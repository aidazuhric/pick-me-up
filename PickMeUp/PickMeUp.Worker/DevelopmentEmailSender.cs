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

        public Task SendAsync(string toEmail, string toName, string subject, string body, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "SMTP is not fully configured (host, username and password are all required), so this message was written to the log instead of being sent.\nTo: {Name} <{Email}>\nSubject: {Subject}\n{Body}",
                toName, toEmail, subject, body);

            return Task.CompletedTask;
        }
    }
}
