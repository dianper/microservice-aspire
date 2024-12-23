using Asp.Versioning;
using Microservice.Aspire.Api.Models;
using Microservice.Aspire.Api.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Microservice.Aspire.Api.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{apiVersion}/files")]
    public class FileController : ControllerBase
    {
        private readonly IDatabase _database;
        private readonly AzureBlobStorageService _azureBlobStorageService;
        private readonly AzureServiceBusService _azureServiceBusService;
        private readonly ILogger<FileController> _logger;

        public FileController(
            IConnectionMultiplexer connectionMultiplexer,
            AzureBlobStorageService azureBlobStorageService,
            AzureServiceBusService azureServiceBusService,
            ILogger<FileController> logger)
        {
            _database = connectionMultiplexer.GetDatabase();
            _azureBlobStorageService = azureBlobStorageService;
            _azureServiceBusService = azureServiceBusService;
            _logger = logger;
        }

        //[HttpGet("get")]
        //public async Task<IEnumerable<WeatherForecast>> GetAsync()
        //{
        //    var cacheValue = await _database.StringGetAsync("weather");

        //    if (cacheValue.HasValue)
        //    {
        //        return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(cacheValue);
        //    }

        //    var weatherForecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();

        //    // Refresh the cache every 15 seconds
        //    await _database.StringSetAsync(
        //        key: "weather",
        //        value: JsonSerializer.Serialize(weatherForecast),
        //        expiry: TimeSpan.FromSeconds(15));

        //    return weatherForecast;
        //}

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync(IFormFile file, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(file, nameof(file));

            if (file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            // Upload the file to Azure Blob Storage
            var blobResponse = await _azureBlobStorageService.UploadAsync(file, cancellationToken);
            if (!blobResponse.IsValid)
            {
                return StatusCode(500, blobResponse);
            }

            // Send a message to the Azure Service Bus
            await _azureServiceBusService.SendAsync(new FileModel(file.FileName, blobResponse.BlobUri), cancellationToken);

            return Ok(blobResponse);
        }
    }
}
