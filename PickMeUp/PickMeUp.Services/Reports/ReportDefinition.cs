namespace PickMeUp.Services.Reports
{
    public class ReportDefinition
    {
        public string Title { get; set; } = string.Empty;

        public string PeriodLabel { get; set; } = string.Empty;

        public string? Scope { get; set; }

        public List<string> Summary { get; set; } = new();

        public List<ReportTable> Tables { get; set; } = new();
    }

    public class ReportTable
    {
        public string Caption { get; set; } = string.Empty;

        public List<ReportColumn> Columns { get; set; } = new();

        public List<List<string>> Rows { get; set; } = new();

        public string EmptyMessage { get; set; } = "No data for the selected period.";
    }

    public class ReportColumn
    {
        public ReportColumn(string header, int relativeWidth = 1, bool alignRight = false)
        {
            Header = header;
            RelativeWidth = relativeWidth;
            AlignRight = alignRight;
        }

        public string Header { get; }

        public int RelativeWidth { get; }

        public bool AlignRight { get; }
    }
}
