namespace Microservice.Aspire.Api.Services;

using Microservice.Aspire.Api.Models;
using System.Text.Json;

public class CovidFileProcessingService(
    AzureBlobStorageService azureBlobStorageService,
    AzureServiceBusService azureServiceBusService,
    CsvReaderService csvReaderService,
    MongoDbService mongoDbService,
    ILogger<CovidFileProcessingService> logger) : BackgroundService
{
    private readonly AzureBlobStorageService _azureBlobStorageService = azureBlobStorageService;
    private readonly AzureServiceBusService _azureServiceBusService = azureServiceBusService;
    private readonly CsvReaderService _csvReaderService = csvReaderService;
    private readonly MongoDbService _mongoDbService = mongoDbService;
    private readonly ILogger<CovidFileProcessingService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for 30 seconds before starting the message processing
        // This is to ensure that the service bus emulator is running
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        // Create a receiver
        var receiver = _azureServiceBusService.CreateReceiver("queue.1");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Receive the message
                var message = await _azureServiceBusService.ReceiveMessageAsync(receiver, stoppingToken);
                if (message == null)
                {
                    _logger.LogInformation("No message received");
                    // Wait for 15 seconds before processing the next message
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }

                _logger.LogInformation($"Received message: {message.Body}");

                // Deserialize the message
                var file = JsonSerializer.Deserialize<FileModel>(message.Body.ToString());
                if (file == null)
                {
                    _logger.LogError("Invalid message received");
                    await _azureServiceBusService.DeadLetterMessageAsync(receiver, message);
                    continue;
                }

                // Download the file
                var response = await _azureBlobStorageService.DownloadAsync(file.Name, stoppingToken);
                if (response.FileData == null)
                {
                    _logger.LogError("An error occurred while downloading the file");
                    await _azureServiceBusService.DeadLetterMessageAsync(receiver, message);
                    continue;
                }

                // Read the CSV file
                var records = _csvReaderService.Read<CovidGlobalDailyModel>(response.FileData);

                // Save data to the database
                await _mongoDbService.SaveManyAsync("covid", "globaldaily", records);

                // Complete the message
                await _azureServiceBusService.CompleteMessageAsync(receiver, message);
            }
        }
        finally
        {
            await receiver.DisposeAsync();
        }
    }
}
