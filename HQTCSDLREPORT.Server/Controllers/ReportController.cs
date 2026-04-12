using HQTCSDL.Models.Report;
using HQTCSDL.Services;
using HQTCSDLREPORT.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HQTCSDLREPORT.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly MetadataService _metadataService;
        private readonly SqlReportStore _sqlReportStore;

        public ReportController(MetadataService metadataService, SqlReportStore sqlReportStore)
        {
            _metadataService = metadataService;
            _sqlReportStore = sqlReportStore;
        }

        [HttpPost("prepare")]
        public IActionResult Prepare([FromBody] ExecuteSqlRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Server) || string.IsNullOrWhiteSpace(model.Database))
            {
                return BadRequest(new { message = "Server and Database are required." });
            }

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = model.Server,
                InitialCatalog = model.Database,
                IntegratedSecurity = true,
                TrustServerCertificate = true
            };

            try
            {
                var dataTable = _metadataService.ExecuteSelectQueryAsDataTable(builder.ConnectionString, model.Sql);
                var reportUrl = _sqlReportStore.Save(
                    dataTable,
                    model.Sql,
                    model.Server,
                    model.Database,
                    model.Title,
                    model.Parameters,
                    model.GroupOrder);

                return Ok(new PrepareReportResponse
                {
                    ReportUrl = reportUrl,
                    RowCount = dataTable.Rows.Count,
                    Columns = dataTable.Columns.Cast<System.Data.DataColumn>().Select(x => x.ColumnName).ToList()
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
