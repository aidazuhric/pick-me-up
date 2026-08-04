using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class City
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
