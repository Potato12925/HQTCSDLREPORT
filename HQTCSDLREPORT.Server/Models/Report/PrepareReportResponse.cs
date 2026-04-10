namespace HQTCSDL.Models.Report
{
    public class PrepareReportResponse
    {
        public string ReportUrl { get; set; } = string.Empty;

        public int RowCount { get; set; }

        public List<string> Columns { get; set; } = new();
    }
}
