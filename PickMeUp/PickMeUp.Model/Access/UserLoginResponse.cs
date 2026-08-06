namespace PickMeUp.Model.Access
{
    public class UserLoginResponse
    {
        public string? Accesstoken { get; set; }
        public string? Refreshtoken { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string? MaskedEmail { get; set; }
        public int? CodeExpiresInSeconds { get; set; }
    }
}
