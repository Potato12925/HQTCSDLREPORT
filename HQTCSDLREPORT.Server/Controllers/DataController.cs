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

        [HttpPost("test")]
        public IActionResult Test([FromBody] DbConnectionModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Server) || string.IsNullOrWhiteSpace(model.Database))
            {
                return BadRequest(new { message = "Server/Database is required." });
            }

            string connectionString =
                $"Server={model.Server};Database={model.Database};User Id=sa;Password=123;TrustServerCertificate=True;";

            if (!_metadataService.TestConnection(connectionString))
            {
                return BadRequest(new { message = "Connection failed." });
            }

            return Ok(new { message = "Test API" });
        }

        [HttpGet("databases")]
        public IActionResult GetDatabases([FromQuery] string server)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                return BadRequest(new { message = "Server is required." });
            }

            string connectionString=$"Server={server};User Id=sa;Password=123;TrustServerCertificate=True;";

            try
            {
                var databases = _metadataService.GetDatabases(connectionString);
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
            string connectionString=$"Server={model.Server};Database={model.Database};User Id=sa;Password=123;TrustServerCertificate=True;";
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

            string connectionString=$"Server={model.Server};Database={model.Database};User Id=sa;Password=123;TrustServerCertificate=True;";

            try
            {
                var result = _metadataService.ExecuteSelectQuery(connectionString, model.Sql);
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
