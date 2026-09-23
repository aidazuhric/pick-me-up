namespace PickMeUp.Model.Responses
{
    public class DriverResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public DateTime? ResignedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? InitialLoginUsername { get; set; }
    }
}
