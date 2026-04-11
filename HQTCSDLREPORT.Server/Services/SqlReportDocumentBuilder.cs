using DevExpress.DataAccess.Json;
using DevExpress.XtraReports.UI;
using System.Data;
using System.Text.Json;

namespace HQTCSDLREPORT.Server.Services
{
    public static class SqlReportDocumentBuilder
    {
        public static XtraReport Build(SqlReportStore.SqlReportItem reportItem, string reportUrl)
        {
            var report = new Report(reportItem.DataTable)
            {
                DisplayName = reportUrl
            };

            var rows = reportItem.DataTable.Rows
                .Cast<DataRow>()
                .Select(row => reportItem.DataTable.Columns
                    .Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => row[col] == DBNull.Value ? null : row[col]))
                .ToList();

            var json = JsonSerializer.Serialize(rows);
            var jsonDataSource = new JsonDataSource
            {
                Name = "SqlResult",
                JsonSource = new CustomJsonSource(json)
            };
            jsonDataSource.Fill();

            report.ComponentStorage.Add(jsonDataSource);
            report.DataSource = jsonDataSource;

            return report;
        }
    }
}
