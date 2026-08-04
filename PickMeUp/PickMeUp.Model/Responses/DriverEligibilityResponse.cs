namespace PickMeUp.Model.Responses
{
    public class DriverEligibilityResponse
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public bool HasOpenShift { get; set; }
        public bool IsOnBreak { get; set; }
        public bool IsDispatchEligible { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
