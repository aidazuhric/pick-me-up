namespace PickMeUp.Model.Requests
{
    public class VehicleCategoryInsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
