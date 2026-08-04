namespace PickMeUp.Model.Responses
{
    public class ShiftResponse
    {
        public int Id { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsOngoing { get; set; }
        public bool IsOnBreak { get; set; }
        public bool IsDispatchEligible { get; set; }
        public int DurationMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int WorkedMinutes { get; set; }
        public int OrdersHandled { get; set; }
        public List<ShiftBreakResponse> Breaks { get; set; } = new();
    }
}
