namespace PickMeUp.Model.Responses
{
    public class ReportFile
    {
        public ReportFile(string fileName, byte[] content)
        {
            FileName = fileName;
            Content = content;
        }

        public string FileName { get; }

        public byte[] Content { get; }

        public string ContentType => "application/pdf";
    }
}
