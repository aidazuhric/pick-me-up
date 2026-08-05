using PickMeUp.Model.Enums;

namespace PickMeUp.Model.SearchObjects
{
    public class PaymentSearch : BaseSearchObject
    {
        public int? OrderId { get; set; }
        public PaymentStatusEnum? Status { get; set; }
        public bool? IsPaid { get; set; }
        public string? PassengerName { get; set; }
        public ShiftPeriod? Period { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
