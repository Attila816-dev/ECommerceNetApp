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
        private readonly EventBusOptions _options;
        private ServiceBusClient? _client;
        private ServiceBusAdministrationClient? _adminClient;

        public AzureServiceBusFactory(IOptions<EventBusOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            _options = options.Value;
        }

        public ServiceBusClient CreateClient()
        {
            if (_client == null)
            {
                lock (_lock)
                {
                    if (_client == null)
                    {
                        if (string.IsNullOrEmpty(_options.ConnectionString))
                        {
                            throw new InvalidOperationException("Azure Service Bus connection string is not configured");
                        }

                        var clientOptions = new ServiceBusClientOptions
                        {
                            TransportType = ServiceBusTransportType.AmqpWebSockets,
                        };

                        _client = new ServiceBusClient(_options.ConnectionString, clientOptions);
                    }
                }
            }

            return _client;
        }

        public ServiceBusAdministrationClient CreateAdministrationClient()
        {
            if (_adminClient == null)
            {
                if (string.IsNullOrEmpty(_options.ConnectionString))
                {
                    throw new InvalidOperationException("Azure Service Bus connection string is not configured");
                }

                _adminClient = new ServiceBusAdministrationClient(_options.ConnectionString);
            }

            return _adminClient;
        }

        public ServiceBusSender CreateSender()
        {
            if (string.IsNullOrEmpty(_options.TopicName))
            {
                throw new InvalidOperationException("Azure Service Bus topic name is not configured");
            }

            return CreateClient().CreateSender(_options.TopicName);
        }

        public ServiceBusProcessor CreateProcessor(string subscriptionName)
        {
            if (string.IsNullOrEmpty(_options.TopicName))
            {
                throw new InvalidOperationException("Azure Service Bus topic name is not configured");
            }

            var options = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = _options.MaxConcurrentCalls,
                AutoCompleteMessages = false,
                PrefetchCount = _options.MaxConcurrentCalls * 3,
            };

            return CreateClient().CreateProcessor(_options.TopicName, subscriptionName, options);
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
