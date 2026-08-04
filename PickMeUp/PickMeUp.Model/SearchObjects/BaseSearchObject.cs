namespace PickMeUp.Model.SearchObjects
{
    public class BaseSearchObject
    {
        public const int DefaultPageSize = 10;

        public const int MaxPageSize = 100;

        public int? Page { get; set; } = 1;

        public int? PageSize { get; set; } = DefaultPageSize;

        public bool? IncludeTotalCount { get; set; } = false;

        public string? SortBy { get; set; }
    }
}
