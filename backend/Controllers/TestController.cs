using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Flightspark.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Pong");
        }

        [HttpGet("echo")]
        public IActionResult Echo([FromQuery] string message)
        {
            return Ok(new { message });
        }
    }
}
