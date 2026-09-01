using System.ComponentModel.DataAnnotations;

namespace PickMeUp.Services.Database
{
    public class Order : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        public int PassengerId { get; set; }

        public User Passenger { get; set; } = null!;

        public int? CityId { get; set; }

        public City? City { get; set; }

        public int? TaxiCompanyId { get; set; }

        public TaxiCompany? TaxiCompany { get; set; }

        public int? DriverId { get; set; }

        public Driver? Driver { get; set; }

        public int? CarId { get; set; }

        public Car? Car { get; set; }

        public double PickupLat { get; set; }

        public double PickupLng { get; set; }

        [Required]
        [MaxLength(250)]
        public string PickupAddress { get; set; } = string.Empty;

        public double? DestinationLat { get; set; }

        public double? DestinationLng { get; set; }

        [MaxLength(250)]
        public string? DestinationAddress { get; set; }

        public decimal? DistanceKm { get; set; }

        public decimal? Fare { get; set; }

        public int OrderStatusId { get; set; }

        public OrderStatus OrderStatus { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AcceptedAt { get; set; }

        public DateTime? PickedUpAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

        public ICollection<OrderRejection> Rejections { get; set; } = new List<OrderRejection>();
    }
}
