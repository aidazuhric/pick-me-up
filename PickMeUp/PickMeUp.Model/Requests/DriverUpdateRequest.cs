namespace PickMeUp.Model.Requests
{
    public class DriverUpdateRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? HireDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
