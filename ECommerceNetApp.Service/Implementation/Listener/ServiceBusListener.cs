using Azure.Messaging.ServiceBus;
using ECommerceNetApp.Service.Interfaces.Listener;

namespace ECommerceNetApp.Service.Implementation.Listener
{
    public class ServiceBusListener : IServiceBusListener, IAsyncDisposable
    {
        private readonly ServiceBusProcessor _processor;
        private readonly ServiceBusClient _client;

        public ServiceBusListener(string connectionString, string topicName, string subscriptionName)
        {
            _client = new ServiceBusClient(connectionString);
            _processor = _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());
        }

        public void StartListening(Func<ServiceBusReceivedMessage, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler)
        {
            _processor.ProcessMessageAsync += async args =>
            {
                await messageHandler(args.Message).ConfigureAwait(false);
                await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
            };

            _processor.ProcessErrorAsync += errorHandler;

            _processor.StartProcessingAsync();
        }

        public async Task StopListeningAsync()
        {
            await _processor.StopProcessingAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_processor != null)
            {
                await _processor.DisposeAsync().ConfigureAwait(false);
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
