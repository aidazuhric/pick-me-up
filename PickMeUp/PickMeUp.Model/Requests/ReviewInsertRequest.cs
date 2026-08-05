namespace PickMeUp.Model.Requests
{
    public class ReviewInsertRequest
    {
        public int OrderId { get; set; }
        public bool ReviewCompany { get; set; }
        public bool ReviewDriver { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
