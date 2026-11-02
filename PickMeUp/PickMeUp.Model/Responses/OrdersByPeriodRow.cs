namespace PickMeUp.Model.Responses
{
    public class OrdersByPeriodRow
    {
        public DateTime PeriodStart { get; set; }

        public string CityName { get; set; } = string.Empty;

        public int OrderCount { get; set; }

        public int CancelledCount { get; set; }

        public decimal Charged { get; set; }

        public decimal Refunded { get; set; }

        public decimal Revenue { get; set; }
    }
}
