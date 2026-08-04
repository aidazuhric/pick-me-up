using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class Shift : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        public int DriverId { get; set; }

        public Driver Driver { get; set; } = null!;

        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public int OrdersHandled { get; set; }

        public ICollection<ShiftBreak> Breaks { get; set; } = new List<ShiftBreak>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
