namespace Microservice.Aspire.Api.Services;

using Microservice.Aspire.Api.Configurations;
using Microservice.Aspire.Api.Constants;
using Microservice.Aspire.Api.Models;
using Microservice.Aspire.Api.Models.Extensions;
using Microsoft.Extensions.Options;
using System.Text.Json;

public class FileDetailsService(
    AzureBlobStorageService azureBlobStorageService,
    AzureServiceBusService azureServiceBusService,
    CsvReaderService csvReaderService,
    MongoDbService mongoDbService,
    IOptions<MongoDbSettings> mongoDbSettingsOptions,
    IOptions<ServiceBusSettings> serviceBusSettingsOptions,
    ILogger<FileDetailsService> logger) : BackgroundService
{
    private readonly AzureBlobStorageService _azureBlobStorageService = azureBlobStorageService;
    private readonly AzureServiceBusService _azureServiceBusService = azureServiceBusService;
    private readonly CsvReaderService _csvReaderService = csvReaderService;
    private readonly MongoDbService _mongoDbService = mongoDbService;

    private readonly MongoDbSettings _mongoDbSettings = mongoDbSettingsOptions.Value;
    private readonly ServiceBusSettings _serviceBusSettings = serviceBusSettingsOptions.Value;

    private readonly ILogger<FileDetailsService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for 30 seconds before starting the message processing
        // This is to ensure that the service bus emulator is running
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        _logger.LogInformation("File details service is running");

        // Create a receiver
        var receiver = _azureServiceBusService.CreateReceiver(_serviceBusSettings.GlobalDetailsQueue);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Receive the message
                var message = await _azureServiceBusService.ReceiveMessageAsync(receiver, stoppingToken);
                if (message is null)
                {
                    _logger.LogInformation("No message received");
                    // Wait for 15 seconds before processing the next message
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                    continue;
                }

                // Deserialize the message
                var file = JsonSerializer.Deserialize<FileModel>(message.Body.ToString());
                if (file is null || string.IsNullOrWhiteSpace(file.Name))
                {
                    _logger.LogError("Invalid message received");
                    await _azureServiceBusService.DeadLetterMessageAsync(receiver, message);
                    continue;
                }

                // Download the file
                var response = await _azureBlobStorageService.DownloadAsync(file.Name, stoppingToken);
                if (response.FileData is null)
                {
                    _logger.LogError("An error occurred while downloading the file");
                    await _azureServiceBusService.DeadLetterMessageAsync(receiver, message);
                    continue;
                }

                // Read the CSV file
                var records = _csvReaderService.Read<GlobalDetailsModel>(response.FileData);
                records.SetIdentifiers(file.Identifier, file.Name);

                // Save data to the database
                await _mongoDbService.InsertManyAsync(_mongoDbSettings.GlobalDetailsCollection, records);

                // Send a message to the next queue
                file.Status = FileStatus.Processed;

                await _azureServiceBusService.SendAsync(
                    queueName: _serviceBusSettings.GlobalSummaryQueue,
                    message: file,
                    cancellationToken: stoppingToken);

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
