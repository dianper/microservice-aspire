
using Azure.Messaging.ServiceBus;
using Microservice.Aspire.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace Microservice.Aspire.Api.Services
{
    public class MessageProcessingService : BackgroundService
    {
        private readonly AzureBlobStorageService _azureBlobStorageService;
        private readonly ServiceBusClient _azureServiceBusClient;
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<MessageProcessingService> _logger;

        public MessageProcessingService(
            AzureBlobStorageService azureBlobStorageService,
            ServiceBusClient serviceBusClient,
            IMongoClient mongoClient,
            ILogger<MessageProcessingService> logger)
        {
            _azureBlobStorageService = azureBlobStorageService ?? throw new ArgumentNullException(nameof(azureBlobStorageService));
            _azureServiceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for 30 seconds before starting the message processing
            // This is to ensure that the service bus emulator is running
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            var receiver = _azureServiceBusClient.CreateReceiver("queue.1");

            await foreach (var message in receiver.ReceiveMessagesAsync(stoppingToken))
            {
                _logger.LogInformation($"Received message: {message.Body}");

                var file = JsonSerializer.Deserialize<FileModel>(message.Body.ToString());

                if (file == null)
                {
                    await receiver.DeadLetterMessageAsync(message);

                    continue;
                }

                var response = await _azureBlobStorageService.DownloadAsync(file.Name, stoppingToken);

                // Save data to the database
                var database = _mongoClient.GetDatabase("filedb");
                var collection = database.GetCollection<BsonDocument>("files");
                var document = new BsonDocument
                {
                    { "messageId", message.MessageId },
                    { "fileName", file.Name },
                    { "fileData", new BsonBinaryData(response.FileData) },
                    { "blobUri", file.BlobUri }
                };

                await collection.InsertOneAsync(document);

                await receiver.CompleteMessageAsync(message);
            }
        }
    }
}
