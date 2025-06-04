using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using ECommerceNetApp.Domain.Options;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Service.Implementation.EventBus
{
    /// <summary>
    /// Factory for creating Azure Service Bus clients, useful for testability and resource management.
    /// </summary>
    public class AzureServiceBusFactory : IAsyncDisposable
    {
        private readonly object _lock = new();
        private readonly AzureEventBusOptions _azureEventBusOptions;
        private readonly ServiceBusProcessorOptions _serviceBusProcessorOptions;
        private ServiceBusClient? _client;
        private ServiceBusAdministrationClient? _adminClient;

        public AzureServiceBusFactory(IOptions<EventBusOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _azureEventBusOptions = options.Value.AzureOptions ?? throw new InvalidOperationException("Azure Service Bus options are not configured");
            _serviceBusProcessorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = options.Value.MaxConcurrentCalls,
                AutoCompleteMessages = false,
                PrefetchCount = options.Value.MaxConcurrentCalls * 3,
            };
        }

        public ServiceBusClient CreateClient()
        {
            if (_client == null)
            {
                lock (_lock)
                {
                    if (_client == null)
                    {
                        if (string.IsNullOrEmpty(_azureEventBusOptions.ConnectionString))
                        {
                            throw new InvalidOperationException("Azure Service Bus connection string is not configured");
                        }

                        var clientOptions = new ServiceBusClientOptions
                        {
                            TransportType = ServiceBusTransportType.AmqpWebSockets,
                        };

                        _client = new ServiceBusClient(_azureEventBusOptions.ConnectionString, clientOptions);
                    }
                }
            }

            return _client;
        }

        public ServiceBusAdministrationClient CreateAdministrationClient()
        {
            if (_adminClient == null)
            {
                if (string.IsNullOrEmpty(_azureEventBusOptions.ConnectionString))
                {
                    throw new InvalidOperationException("Azure Service Bus connection string is not configured");
                }

                _adminClient = new ServiceBusAdministrationClient(_azureEventBusOptions.ConnectionString);
            }

            return _adminClient;
        }

        public ServiceBusSender CreateSender()
        {
            if (string.IsNullOrEmpty(_azureEventBusOptions.TopicName))
            {
                throw new InvalidOperationException("Azure Service Bus topic name is not configured");
            }

            return CreateClient().CreateSender(_azureEventBusOptions.TopicName);
        }

        public ServiceBusProcessor CreateProcessor(string subscriptionName)
        {
            if (string.IsNullOrEmpty(_azureEventBusOptions.TopicName))
            {
                throw new InvalidOperationException("Azure Service Bus topic name is not configured");
            }

            return CreateClient().CreateProcessor(_azureEventBusOptions.TopicName, subscriptionName, _serviceBusProcessorOptions);
        }

        public async ValueTask DisposeAsync()
        {
            if (_client != null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
                _client = null;
            }

            _adminClient = null;

            // Suppress finalization to prevent derived types with finalizers from needing to re-implement IDisposable
            GC.SuppressFinalize(this);
        }
    }
}
