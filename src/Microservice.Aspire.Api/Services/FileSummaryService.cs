namespace Microservice.Aspire.Api.Services;

using Microservice.Aspire.Api.Configurations;
using Microservice.Aspire.Api.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

public class FileSummaryService(
    AzureServiceBusService azureServiceBusService,
    MongoDbService mongoDbService,
    RedisService redisService,
    IOptions<MongoDbSettings> mongoDbSettingsOptions,
    IOptions<ServiceBusSettings> serviceBusSettingsOptions,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<FileSummaryService> logger) : BackgroundService
{
    private readonly AzureServiceBusService _azureServiceBusService = azureServiceBusService;
    private readonly MongoDbService _mongoDbService = mongoDbService;
    private readonly RedisService _redisService = redisService;

    private readonly MongoDbSettings _mongoDbSettings = mongoDbSettingsOptions.Value;
    private readonly ServiceBusSettings _serviceBusSettings = serviceBusSettingsOptions.Value;

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<FileSummaryService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for 30 seconds before starting the message processing
        // This is to ensure that the service bus emulator is running
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        _logger.LogInformation("File summary service is running");

        // Create a receiver
        var receiver = _azureServiceBusService.CreateReceiver(_serviceBusSettings.GlobalSummaryQueue);

        // Create a scope to resolve the Postgres service
        using var scope = _serviceScopeFactory.CreateScope();
        var postgresService = scope.ServiceProvider.GetRequiredService<PostgresService>();

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

            try
            {
                // Get aggregate data from MongoDB
                var summaries = await _mongoDbService.GetGlobalSummaryAsync(
                    collectionName: _mongoDbSettings.GlobalDetailsCollection,
                    identifier: file.Identifier);

                // Save the aggregate data to Postgres
                await postgresService.InsertGlobalSummariesAsync(summaries, stoppingToken);

                // Save the aggregate data to Redis
                await _redisService.SetAsync("globalsummaries", summaries);
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while saving the data to Postgres");
                await _azureServiceBusService.DeadLetterMessageAsync(receiver, message);
                continue;
            }

            // Complete the message
            await _azureServiceBusService.CompleteMessageAsync(receiver, message);
        }
    }
}
