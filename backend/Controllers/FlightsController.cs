using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Flightspark.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly ILogger<FlightsController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FlightsController(ILogger<FlightsController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetFlights(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] string date,
            [FromQuery] string? dateBack, //  dateBack необязательный
            [FromQuery] int adults)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || string.IsNullOrEmpty(date) || adults <= 0)
            {
                return BadRequest(new { error = "Please provide from_city, to_city, and date parameters" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("apikey", _configuration["API_KEY"]);

                var url = $"https://api.tequila.kiwi.com/v2/search?fly_from={from}&fly_to={to}&date_from={date}&date_to={date}&adults={adults}&max_stopovers=2&limit=30";
                
                if (!string.IsNullOrEmpty(dateBack))
                {
                    url += $"&return_from={dateBack}&return_to={dateBack}";
                }

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { error = "Failed to fetch flights" });
                }

                var content = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(content)["data"];

                var results = new List<object>();

                foreach (var flightData in data)
                {
                    var formattedData = new
                    {
                        adults,
                        price = flightData["price"]?.ToString() ?? "N/A",
                        url = flightData["deep_link"]?.ToString() ?? "N/A",
                        from = new
                        {
                            city = flightData["cityFrom"]?.ToString() ?? "N/A",
                            city_code = flightData["cityCodeFrom"]?.ToString() ?? "N/A",
                            country = flightData["countryFrom"]?["name"]?.ToString() ?? "N/A"
                        },
                        to = new
                        {
                            city = flightData["cityTo"]?.ToString() ?? "N/A",
                            city_code = flightData["cityCodeTo"]?.ToString() ?? "N/A",
                            country = flightData["countryTo"]?["name"]?.ToString() ?? "N/A"
                        },
                        outbound_routes = new List<object>(),
                        return_routes = new List<object>()
                    };

                    foreach (var route in flightData["route"])
                    {
                        var routeData = new
                        {
                            airline = route["airline"]?.ToString() ?? "N/A",
                            from = route["cityFrom"]?.ToString() ?? "N/A",
                            to = route["cityTo"]?.ToString() ?? "N/A",
                            departure = route["local_departure"]?.ToString() ?? "N/A",
                            arrival = route["local_arrival"]?.ToString() ?? "N/A"
                        };

                        if ((int)route["return"] == 0)
                        {
                            ((List<object>)formattedData.outbound_routes).Add(routeData);
                        }
                        else
                        {
                            ((List<object>)formattedData.return_routes).Add(routeData);
                        }
                    }

                    results.Add(formattedData);
                }

                return Ok(results);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"HttpRequestException: {e.Message}");
                return StatusCode(500, new { error = "Aiohttp Client Error" });
            }
            catch (System.Exception e)
            {
                _logger.LogError($"Exception: {e.Message}");
                return StatusCode(500, new { error = "Server Error" });
            }
        }
    }
}
