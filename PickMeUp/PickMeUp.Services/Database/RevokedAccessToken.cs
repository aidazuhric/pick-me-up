using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class RevokedAccessToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string TokenId { get; set; } = string.Empty;

        public int UserId { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    }
}
