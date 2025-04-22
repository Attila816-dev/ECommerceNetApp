using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ECommerceNetApp.Service.Interfaces.Publisher;

namespace ECommerceNetApp.Service.Implementation.Publisher
{
    public class ServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public ServiceBusPublisher(string connectionString, string topicName)
        {
            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(topicName);
        }

        public async Task PublishMessageAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                ApplicationProperties = { ["EventType"] = typeof(T).Name },
            };

            await _sender.SendMessageAsync(serviceBusMessage, cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_sender != null)
            {
                await _sender.DisposeAsync().ConfigureAwait(false);
            }

            if (_client != null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
            }

            // Suppress finalization to prevent derived types with finalizers from needing to re-implement IDisposable.
            GC.SuppressFinalize(this);
        }
    }
}
