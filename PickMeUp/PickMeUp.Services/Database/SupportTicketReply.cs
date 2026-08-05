using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class SupportTicketReply
    {
        [Key]
        public int Id { get; set; }

        public int SupportTicketId { get; set; }

        public SupportTicket SupportTicket { get; set; } = null!;

        public int AuthorId { get; set; }

        public User Author { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Body { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
