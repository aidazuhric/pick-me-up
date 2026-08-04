namespace PickMeUp.Model.Requests
{
    public class VehicleCategoryUpdateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
