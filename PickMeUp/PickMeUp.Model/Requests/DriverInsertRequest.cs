namespace PickMeUp.Model.Requests
{
    public class DriverInsertRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; } = true;

        public string LoginUsername { get; set; } = string.Empty;
        public string LoginPassword { get; set; } = string.Empty;
    }
}
