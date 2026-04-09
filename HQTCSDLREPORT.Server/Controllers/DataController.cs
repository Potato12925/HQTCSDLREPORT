using HQTCSDL.Models;
using HQTCSDL.Models.Report;
using HQTCSDL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HQTCSDL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly MetadataService _metadataService;

        public DataController()
        {
            _metadataService = new MetadataService();
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Test API" });
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

        [HttpGet("report")]
        public IActionResult Report()
        {
            return Ok(new { message = "Report API" });
        }
    }
}
