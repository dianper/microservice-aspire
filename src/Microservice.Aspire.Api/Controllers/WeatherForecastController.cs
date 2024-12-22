using Asp.Versioning;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace Microservice.Aspire.Api.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{apiVersion}/weatherforecasts")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IDatabase _database;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ServiceBusClient _busClient;
        private readonly BlobServiceClient _blobServiceClient;

        public WeatherForecastController(
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<WeatherForecastController> logger,
            ServiceBusClient busClient,
            BlobServiceClient blobClient)
        {
            _database = connectionMultiplexer.GetDatabase();
            _logger = logger;
            _busClient = busClient;
            _blobServiceClient = blobClient;
        }

        [HttpGet("get")]
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

        [HttpPost("add")]
        public async Task<IActionResult> AddMessageAsync(WeatherForecast weatherForecast)
        {
            var sender = _busClient.CreateSender("queue.1");
            var message = new ServiceBusMessage(JsonSerializer.Serialize(weatherForecast));
            await sender.SendMessageAsync(message);
            return Ok();
        }

        [HttpGet("read")]
        public async Task<IActionResult> ReadMessageAsync()
        {
            var receiver = _busClient.CreateReceiver("queue.1");

            var messages = new List<WeatherForecast>();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages: 10, cancellationToken: cts.Token);

            foreach (var message in receivedMessages)
            {
                var body = message.Body.ToString();

                var weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(body);

                messages.Add(weatherForecast);

                await receiver.CompleteMessageAsync(message);
            }

            return Ok(messages);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync()
        {
            var blobContainerClient = _blobServiceClient
                .GetBlobContainerClient("files");

            await blobContainerClient
                .CreateIfNotExistsAsync();

            var blobClient = blobContainerClient
                .GetBlobClient("weather.json");

            var weatherForecast = new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };

            var json = JsonSerializer.Serialize(weatherForecast);

            await blobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(json)), overwrite: true);

            return Ok();
        }
    }
}
