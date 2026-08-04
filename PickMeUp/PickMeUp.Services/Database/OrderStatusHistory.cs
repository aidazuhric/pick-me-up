using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class OrderStatusHistory
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        public int? FromStatusId { get; set; }

        public OrderStatus? FromStatus { get; set; }

        public int ToStatusId { get; set; }

        public OrderStatus ToStatus { get; set; } = null!;

        public int ChangedByUserId { get; set; }

        public User ChangedByUser { get; set; } = null!;

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
