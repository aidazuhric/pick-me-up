using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class PaymentStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
