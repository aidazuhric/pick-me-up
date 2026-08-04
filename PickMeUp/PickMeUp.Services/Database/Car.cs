using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class Car : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(60)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Plate { get; set; } = string.Empty;

        public int Year { get; set; }

        public int Seats { get; set; }

        public int VehicleCategoryId { get; set; }

        public VehicleCategory VehicleCategory { get; set; } = null!;

        public int TaxiCompanyId { get; set; }

        public TaxiCompany TaxiCompany { get; set; } = null!;

        [MaxLength(260)]
        public string? PhotoImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
