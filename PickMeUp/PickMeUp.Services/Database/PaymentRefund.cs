using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class PaymentRefund
    {
        [Key]
        public int Id { get; set; }

        public int PaymentId { get; set; }

        public Payment Payment { get; set; } = null!;

        public decimal RefundedAmount { get; set; }

        [MaxLength(120)]
        public string? StripeRefundId { get; set; }

        public Guid? ClientRefundKey { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public DateTime RefundedAt { get; set; } = DateTime.UtcNow;
    }
}
