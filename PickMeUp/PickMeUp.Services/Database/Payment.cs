using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class Payment : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;

        public decimal AmountCharged { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "BAM";

        public int PaymentStatusId { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = null!;

        [MaxLength(120)]
        public string? StripePaymentIntentId { get; set; }

        public bool IsPaid { get; set; }

        public int IntentAttempt { get; set; }

        public bool RefundRequired { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public ICollection<PaymentRefund> Refunds { get; set; } = new List<PaymentRefund>();
    }
}
