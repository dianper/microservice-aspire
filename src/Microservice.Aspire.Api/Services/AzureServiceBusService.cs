using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Microservice.Aspire.Api.Services
{
    public class AzureServiceBusService
    {
        private readonly ServiceBusClient _serviceBusClient;

        public AzureServiceBusService(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        }

        public async Task SendAsync<T>(T message, CancellationToken cancellationToken)
            where T : class
        {
            try
            {
                var sender = _serviceBusClient.CreateSender("queue.1");
                var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
                await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while sending the message", ex);
            }
        }
    }
}
