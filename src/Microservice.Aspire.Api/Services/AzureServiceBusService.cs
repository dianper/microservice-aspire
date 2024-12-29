namespace Microservice.Aspire.Api.Services;

using Azure.Messaging.ServiceBus;
using System.Text.Json;

public class AzureServiceBusService(ServiceBusClient serviceBusClient)
{
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;

    public ServiceBusReceiver CreateReceiver(string queueName)
    {
        return _serviceBusClient.CreateReceiver(queueName);
    }

    public async Task CompleteMessageAsync(ServiceBusReceiver receiver, ServiceBusReceivedMessage message)
    {
        await receiver.CompleteMessageAsync(message);
    }

    public async Task DeadLetterMessageAsync(ServiceBusReceiver receiver, ServiceBusReceivedMessage message)
    {
        await receiver.DeadLetterMessageAsync(message);
    }

    public async Task<ServiceBusReceivedMessage> ReceiveMessageAsync(ServiceBusReceiver receiver, CancellationToken cancellationToken)
    {
        return await receiver.ReceiveMessageAsync(cancellationToken: cancellationToken);
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
