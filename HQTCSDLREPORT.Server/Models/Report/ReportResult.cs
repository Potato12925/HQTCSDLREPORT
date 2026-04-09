namespace HQTCSDL.Models.Report
{
    public class ReportResult
    {
        public List<string> Columns { get; set; } = new List<string>();

        public List<Dictionary<string, object?>> Rows { get; set; } = new List<Dictionary<string, object?>>();
    }
}
