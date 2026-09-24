namespace PickMeUp.Worker
{
    public interface IEmailSender
    {
        Task SendAsync(
            string toEmail,
            string toName,
            string subject,
            string body,
            string htmlBody,
            CancellationToken cancellationToken);
    }
}
