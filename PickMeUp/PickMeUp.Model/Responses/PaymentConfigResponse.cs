namespace PickMeUp.Model.Responses
{
    public class PaymentConfigResponse
    {
        public string PublishableKey { get; set; } = string.Empty;

        public bool IsLiveGateway { get; set; }
    }
}
