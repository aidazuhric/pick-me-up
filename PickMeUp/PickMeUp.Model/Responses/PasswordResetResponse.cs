namespace PickMeUp.Model.Responses
{
    public class PasswordResetResponse
    {
        public string Message { get; set; } = string.Empty;

        public int ExpiresInMinutes { get; set; }
    }
}
