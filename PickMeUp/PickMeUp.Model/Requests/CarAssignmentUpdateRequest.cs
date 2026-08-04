namespace PickMeUp.Model.Requests
{
    public class CarAssignmentUpdateRequest
    {
        public int? CarId { get; set; }
        public int? DriverId { get; set; }
        public DateTime? AssignedFrom { get; set; }
        public DateTime? AssignedTo { get; set; }
        public bool? IsActive { get; set; }
    }
}
