namespace PickMeUp.Model.Responses
{
    public class ShiftBreakResponse
    {
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsOngoing { get; set; }
        public int DurationMinutes { get; set; }
    }
}
