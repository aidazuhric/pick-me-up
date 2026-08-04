using PickMeUp.Model.Enums;

namespace PickMeUp.Model.SearchObjects
{
    public class OrderSearch : BaseSearchObject
    {
        public OrderStatusEnum? Status { get; set; }
        public int? TaxiCompanyId { get; set; }
        public int? DriverId { get; set; }
        public string? PassengerName { get; set; }
        public bool? AvailableForMe { get; set; }
        public ShiftPeriod? Period { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
