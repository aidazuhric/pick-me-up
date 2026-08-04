using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class ShiftBreak
    {
        [Key]
        public int Id { get; set; }

        public int ShiftId { get; set; }

        public Shift Shift { get; set; } = null!;

        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
