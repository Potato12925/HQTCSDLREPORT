namespace HQTCSDL.Models.Report
{
    public class ExecuteSqlRequest
    {
        
        public string Server { get; set; } = string.Empty;

        public string Database { get; set; } = string.Empty;

        public string Sql { get; set; } = string.Empty;
    }
}
