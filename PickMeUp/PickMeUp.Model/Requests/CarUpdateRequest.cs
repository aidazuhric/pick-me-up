namespace PickMeUp.Model.Requests
{
    public class CarUpdateRequest
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Plate { get; set; }
        public int? Year { get; set; }
        public int? Seats { get; set; }
        public int? VehicleCategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}
