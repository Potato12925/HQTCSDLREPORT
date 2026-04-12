using System.ComponentModel;

namespace HQTCSDL.Models.Report
{
    public class ExecuteSqlRequest
    {
        [DefaultValue("(localdb)\\MSSQLLocalDB")]   
        public string Server { get; set; } = string.Empty;
        [DefaultValue("QLVT_DATHANG")]
        public string Database { get; set; } = string.Empty;
        public string Sql { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public List<ReportParameterRequest> Parameters { get; set; } = new();

        public List<ReportGroupOrderRequest> GroupOrder { get; set; } = new();
    }

    public class ReportParameterRequest
    {
        public int? TableId { get; set; }

        public int? ColumnId { get; set; }

        public string ColumnName { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    public class ReportGroupOrderRequest
    {
        public int? Order { get; set; }

        public int? TableId { get; set; }

        public int? ColumnId { get; set; }

        public string ColumnName { get; set; } = string.Empty;
    }
}
