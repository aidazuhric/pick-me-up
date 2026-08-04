namespace PickMeUp.Model.Responses
{
    public class CarAssignmentResponse
    {
        public int Id { get; set; }
        public string CarPlate { get; set; } = string.Empty;
        public string CarMake { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime AssignedFrom { get; set; }
        public DateTime? AssignedTo { get; set; }
        public bool IsOngoing { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
