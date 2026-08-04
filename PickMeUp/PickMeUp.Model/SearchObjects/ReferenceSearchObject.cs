namespace PickMeUp.Model.SearchObjects
{
    public abstract class ReferenceSearchObject : BaseSearchObject
    {
        public string? Name { get; set; }

        public bool? IsActive { get; set; }
    }
}
