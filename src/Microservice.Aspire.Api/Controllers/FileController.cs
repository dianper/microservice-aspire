namespace Microservice.Aspire.Api.Controllers;

using Asp.Versioning;
using Microservice.Aspire.Api.Configurations;
using Microservice.Aspire.Api.Constants;
using Microservice.Aspire.Api.Models;
using Microservice.Aspire.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{apiVersion}/files")]
public class FileController(
    AzureBlobStorageService azureBlobStorageService,
    AzureServiceBusService azureServiceBusService,
    MongoDbService mongoDbService,
    IOptions<FileCollectorSettings> fileSettingsOptions,
    IOptions<MongoDbSettings> mongoDbSettingsOptions,
    IOptions<ServiceBusSettings> serviceBusSettingsOptions,
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<FileController> logger) : ControllerBase
{
    private readonly AzureBlobStorageService _azureBlobStorageService = azureBlobStorageService;
    private readonly AzureServiceBusService _azureServiceBusService = azureServiceBusService;
    private readonly MongoDbService _mongoDbService = mongoDbService;

    private readonly FileCollectorSettings _fileSettings = fileSettingsOptions.Value;
    private readonly MongoDbSettings _mongoDbSettings = mongoDbSettingsOptions.Value;
    private readonly ServiceBusSettings _serviceBusSettings = serviceBusSettingsOptions.Value;

    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly ILogger<FileController> _logger = logger;

    [HttpPost]
    public async Task<IActionResult> PostAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

        if (file.Length == 0)
        {
            return BadRequest("File is empty");
        }

        // Check if the file extension is CSV
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !_fileSettings.AllowedExtensions.Contains(extension.ToLower()))
        {
            return BadRequest("Invalid file extension");
        }

        // Check if the file already exists in the database
        var fileId = Convert.ToBase64String(Encoding.UTF8.GetBytes(file.FileName));
        if (await _mongoDbService.ExistsAsync<GlobalDetailsModel>(
            _mongoDbSettings.GlobalDetailsCollection,
            nameof(GlobalDetailsModel.Identifier),
            fileId))
        {
            _logger.LogInformation("File already processed: {FileName}", file.FileName);
            return Ok();
        }

        // Upload the file to Azure Blob Storage
        var blobResponse = await _azureBlobStorageService.UploadAsync(file, cancellationToken);
        if (!blobResponse.IsValid)
        {
            return StatusCode(500, blobResponse);
        }

        // Send a message to the Azure Service Bus
        await _azureServiceBusService.SendAsync(
            queueName: _serviceBusSettings.GlobalDetailsQueue,
            message: new FileModel(fileId)
            {
                BlobUri = blobResponse.BlobUri,
                Name = file.FileName,
                Status = FileStatus.Uploaded
            },
            cancellationToken);

        return Ok(blobResponse);
    }
}
