using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class TaxiCompany : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string HqAddress { get; set; } = string.Empty;

        public int CityId { get; set; }

        public City City { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string OwnerName { get; set; } = string.Empty;

        public decimal StartingPrice { get; set; }

        public decimal PricePerKm { get; set; }

        [MaxLength(260)]
        public string? LogoImagePath { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
