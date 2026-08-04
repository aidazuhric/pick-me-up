using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class OrderRejection
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        public int DriverId { get; set; }

        public Driver Driver { get; set; } = null!;

        public DateTime RejectedAt { get; set; } = DateTime.UtcNow;
    }
}
