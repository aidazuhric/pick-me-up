using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class Review : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        public int PassengerId { get; set; }

        public User Passenger { get; set; } = null!;

        public int? TargetCompanyId { get; set; }

        public TaxiCompany? TargetCompany { get; set; }

        public int? TargetDriverId { get; set; }

        public Driver? TargetDriver { get; set; }

        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
