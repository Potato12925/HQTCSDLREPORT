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
    }
}
