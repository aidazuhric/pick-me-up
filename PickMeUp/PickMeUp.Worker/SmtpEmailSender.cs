using PickMeUp.Common.Services.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace PickMeUp.Worker
{
    public class SmtpEmailSender : IEmailSender
    {
        private const string DefaultSender = "no-reply@pickmeup.local";

        private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(
            string toEmail,
            string toName,
            string subject,
            string body,
            string htmlBody,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
            {
                throw new InvalidOperationException("SMTP_HOST is not configured, so the notification cannot be delivered.");
            }

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            var from = string.IsNullOrWhiteSpace(_options.Username) ? DefaultSender : _options.Username;

            using var message = new MailMessage
            {
                From = new MailAddress(from, "PickMeUp"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(new MailAddress(toEmail, toName));

            if (!string.IsNullOrWhiteSpace(htmlBody))
            {
                message.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(body, Encoding.UTF8, MediaTypeNames.Text.Plain));
                message.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html));
            }

            await client.SendMailAsync(message, cancellationToken);
        }
    }
}
