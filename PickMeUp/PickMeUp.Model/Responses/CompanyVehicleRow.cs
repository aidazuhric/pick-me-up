namespace PickMeUp.Model.Responses
{
    public class CompanyVehicleRow
    {
        public string Vehicle { get; set; } = string.Empty;

        public string Plate { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public int CompletedRides { get; set; }

        public decimal Revenue { get; set; }
    }
}
