namespace PickMeUp.Model.Access
{
    public class TwoFactorVerifyRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
