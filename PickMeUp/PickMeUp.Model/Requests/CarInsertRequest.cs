namespace PickMeUp.Model.Requests
{
    public class CarInsertRequest
    {
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Seats { get; set; }
        public int VehicleCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
