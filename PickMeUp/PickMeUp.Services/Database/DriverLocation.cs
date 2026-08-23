using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class DriverLocation
    {
        [Key]
        public int DriverId { get; set; }

        public Driver Driver { get; set; } = null!;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
