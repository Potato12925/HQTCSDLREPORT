using HQTCSDL.Models;
using HQTCSDL.Models.Report;
using HQTCSDL.Services;
using HQTCSDLREPORT.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HQTCSDL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly MetadataService _metadataService;

        public DataController(MetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Test API" });
        }

        [HttpGet("databases")]
        public IActionResult GetDatabases([FromQuery] string server)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                return BadRequest(new { message = "Server is required." });
            }

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                IntegratedSecurity = true,
                TrustServerCertificate = true
            };

            try
            {
                var databases = _metadataService.GetDatabases(builder.ConnectionString);
                return Ok(databases);
            }
            catch (SqlException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("connect")]
        public IActionResult Connect([FromBody] DbConnectionModel model)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = model.Server,
                InitialCatalog = model.Database,
                IntegratedSecurity = true,
                TrustServerCertificate = true
            };

            string connectionString = builder.ConnectionString;
            if (!_metadataService.TestConnection(connectionString))
            {
                return BadRequest(new { message = "Connection failed." });
            }

            var metadata = _metadataService.LoadMetadata(connectionString);

            return Ok(metadata);
        }

        [HttpPost("execute")]
        public IActionResult Execute([FromBody] ExecuteSqlRequest model)
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
                var result = _metadataService.ExecuteSelectQuery(builder.ConnectionString, model.Sql);
                return Ok(result);
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
        [HttpPost("report")]
        public IActionResult Report([FromBody] ExecuteSqlRequest model)
        {
            return BadRequest(new { message = "Use /api/Report/prepare to generate report data." });
        }
    }
}
