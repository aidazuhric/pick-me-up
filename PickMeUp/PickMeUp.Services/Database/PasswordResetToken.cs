using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PickMeUp.Services.Database
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string TokenSalt { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? ConsumedAt { get; set; }

        public int AttemptCount { get; set; }
    }
}
