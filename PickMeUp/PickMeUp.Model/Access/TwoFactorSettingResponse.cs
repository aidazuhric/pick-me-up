namespace PickMeUp.Model.Access
{
    public class TwoFactorSettingResponse
    {
        public bool Enabled { get; set; }
        public string MaskedEmail { get; set; } = string.Empty;
    }
}
