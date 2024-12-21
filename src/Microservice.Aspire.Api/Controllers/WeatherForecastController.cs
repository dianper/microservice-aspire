using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace Microservice.Aspire.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _database;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<WeatherForecastController> logger)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _database = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            var cacheValue = await _database.StringGetAsync("weather");

            if (cacheValue.HasValue)
            {
                return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(cacheValue);
            }

            var weatherForecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            // Refresh the cache every 15 seconds
            await _database.StringSetAsync(
                key: "weather",
                value: JsonSerializer.Serialize(weatherForecast),
                expiry: TimeSpan.FromSeconds(15));

            return weatherForecast;
        }
    }
}
