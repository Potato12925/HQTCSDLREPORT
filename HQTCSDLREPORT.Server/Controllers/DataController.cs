using HQTCSDL.Models;
using HQTCSDL.Services;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("query")]
        public IActionResult Query()
        {
            string conn = HttpContext.Session.GetString("ConnectionString");

            if (string.IsNullOrEmpty(conn))
            {
                return BadRequest(new { message = "Chưa kết nối database" });
            }

            var metadata = _metadataService.LoadMetadata(conn);

            return Ok(metadata);
        }

        [HttpPost("connect")]
        public IActionResult Connect([FromBody] DbConnectionModel model)
        {
            string connectionString =
            $"Server={model.Server};Database={model.Database};User Id={model.Username};Password={model.Password};TrustServerCertificate=True;";

            if (!_metadataService.TestConnection(connectionString))
            {
                return BadRequest(new { message = "Kết nối thất bại" });
            }

            var metadata = _metadataService.LoadMetadata(connectionString);

            return Ok(metadata);
        }

        [HttpGet("report")]
        public IActionResult Report()
        {
            return Ok(new { message = "Report API" });
        }
    }
}