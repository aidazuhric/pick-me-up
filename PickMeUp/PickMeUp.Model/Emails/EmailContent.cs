namespace PickMeUp.Model.Emails
{
    public class EmailFact
    {
        public EmailFact(string label, string value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; }

        public string Value { get; }
    }

    public class EmailContent
    {
        public string Title { get; init; } = string.Empty;

        public string Intro { get; init; } = string.Empty;

        public string? Code { get; init; }

        public string? CodeCaption { get; init; }

        public IReadOnlyList<EmailFact> Facts { get; init; } = new List<EmailFact>();

        public string? Highlight { get; init; }

        public string? SecurityNote { get; init; }
    }
}
