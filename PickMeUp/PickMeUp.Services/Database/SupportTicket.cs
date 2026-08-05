using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class SupportTicket : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        public int SubmitterId { get; set; }

        public User Submitter { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Body { get; set; } = string.Empty;

        public int SupportTicketStatusId { get; set; }

        public SupportTicketStatus SupportTicketStatus { get; set; } = null!;

        public int? TargetUserId { get; set; }

        public User? TargetUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public int? ResolvedByUserId { get; set; }

        public User? ResolvedByUser { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public ICollection<SupportTicketReply> Replies { get; set; } = new List<SupportTicketReply>();
    }
}
