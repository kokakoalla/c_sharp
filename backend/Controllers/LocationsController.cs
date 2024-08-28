using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flightspark.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Убедитесь, что здесь используется "controller"
    public class LocationsController : ControllerBase
    {
        private readonly ILogger<LocationsController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LocationsController(ILogger<LocationsController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetLocations([FromQuery] string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return BadRequest(new { error = "Missing term parameter" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("apikey", _configuration["API_KEY"]);

                var response = await client.GetAsync($"{_configuration["TEQUILA_ENDPOINT_LOCATION"]}/locations/query?term={term}&location_types=city&limit=5&active_only=True");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { error = "Failed to fetch locations" });
                }

                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"HttpRequestException: {e.Message}");
                return StatusCode(500, new { error = "Failed to fetch locations" });
            }
        }
    }
}
