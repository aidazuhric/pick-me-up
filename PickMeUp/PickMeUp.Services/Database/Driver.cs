using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class Driver : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int TaxiCompanyId { get; set; }

        public TaxiCompany TaxiCompany { get; set; } = null!;

        public DateTime HireDate { get; set; }

        public DateTime? ResignedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
