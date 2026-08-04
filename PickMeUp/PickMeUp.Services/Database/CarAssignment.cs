using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class CarAssignment : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        public int CarId { get; set; }

        public Car Car { get; set; } = null!;

        public int DriverId { get; set; }

        public Driver Driver { get; set; } = null!;

        public DateTime AssignedFrom { get; set; }

        public DateTime? AssignedTo { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
