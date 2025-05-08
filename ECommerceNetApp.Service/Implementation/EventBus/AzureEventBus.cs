using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Service.Implementation.EventBus
{
    internal sealed class AzureEventBus : IEventBus, IAsyncDisposable
    {
        private static readonly Action<ILogger, string, string, Exception?> LogNotificationHandlerRegistration =
            LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, nameof(AzureEventBus)),
            "Handler {HandlerType} registered for notification {NotificationType}");

        private static readonly Action<ILogger, string, Exception?> LogEventPublishedToAzureServiceBus =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(AzureEventBus)),
            "Event {EventType} published to Azure Service Bus");

        private static readonly Action<ILogger, string, string, Exception?> LogSubscripitionCreated =
            LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, nameof(AzureEventBus)),
            "Created subscription {SubscriptionName} for {EventType}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageReceived =
            LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1, nameof(AzureEventBus)),
            "Received message: {MessageBody} of type {EventType}");

        private static readonly Action<ILogger, string, Exception?> LogStartOfProcessingMessages =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(AzureEventBus)),
            "Started processing messages for subscription {SubscriptionName}");

        private static readonly Action<ILogger, string, Exception?> LogMessageProcessingError =
            LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(AzureEventBus)),
            "Error processing message of type {EventType}");

        private static readonly Action<ILogger, string, Exception?> LogMessageProcessingFromSubscriptionError =
            LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(AzureEventBus)),
            "Error processing messages from subscription {Subscription}");

        private readonly ServiceBusClient _client;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<AzureEventBus> _logger;
        private readonly IOptions<EventBusOptions> _eventBusOptions;
        private readonly Dictionary<Type, List<Func<INotification, CancellationToken, Task>>> _handlers = new();
        private readonly List<ServiceBusProcessor> _processors = new();

        public AzureEventBus(
            IOptions<EventBusOptions> eventBusOptions,
            ILogger<AzureEventBus> logger)
        {
            _eventBusOptions = eventBusOptions;
            _logger = logger;
            _client = new ServiceBusClient(_eventBusOptions.Value.ConnectionString);
            _adminClient = new ServiceBusAdministrationClient(_eventBusOptions.Value.ConnectionString);
            _sender = _client.CreateSender(_eventBusOptions.Value.TopicName);
        }

        public void Register<TNotification>(INotificationHandler<TNotification> handler)
            where TNotification : INotification
        {
            var eventType = typeof(TNotification);

            if (!_handlers.TryGetValue(eventType, out var handlerList))
            {
                handlerList = new();
                _handlers[eventType] = handlerList;
            }

            handlerList.Add((evt, token) => handler.HandleAsync((TNotification)evt, token));
            LogNotificationHandlerRegistration.Invoke(_logger, handler.GetType().Name, typeof(TNotification).Name, null);
        }

        public async Task PublishAsync<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : class, INotification
        {
            var eventTypeName = notification.GetType().Name;
            var messageBody = JsonSerializer.Serialize(notification);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
            {
                Subject = eventTypeName,
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
            };

            await _sender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
            LogEventPublishedToAzureServiceBus.Invoke(_logger, eventTypeName, null);
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
        {
            // For each event type we handle, create a subscription
            foreach (var eventType in _handlers.Keys)
            {
                var subscriptionName = $"{_eventBusOptions.Value.TopicName}-{eventType.Name}";

                // Ensure subscription exists
                if (!await _adminClient.SubscriptionExistsAsync(_eventBusOptions.Value.TopicName, subscriptionName, cancellationToken).ConfigureAwait(false))
                {
                    // Create rule to filter by event type
                    var rule = new CreateRuleOptions(
                        $"{eventType.Name}Rule",
                        new CorrelationRuleFilter { Subject = eventType.Name });

                    await _adminClient.CreateSubscriptionAsync(
                        new CreateSubscriptionOptions(_eventBusOptions.Value.TopicName, subscriptionName),
                        rule,
                        cancellationToken)
                        .ConfigureAwait(false);

                    LogSubscripitionCreated.Invoke(_logger, subscriptionName, eventType.Name, null);
                }

                // Create a processor for this subscription
                var processor = _client.CreateProcessor(_eventBusOptions.Value.TopicName, subscriptionName);

                // Handle messages
                processor.ProcessMessageAsync += async args =>
                {
                    var message = args.Message.Body.ToString();
                    var eventTypeName = args.Message.Subject;

                    LogMessageReceived.Invoke(_logger, message, eventTypeName, null);

                    if (_handlers.TryGetValue(eventType, out var handlers))
                    {
#pragma warning disable CA1031 // Do not catch general exception types
                        try
                        {
                            var notification = JsonSerializer.Deserialize(message, eventType) as INotification;
                            if (notification != null)
                            {
                                foreach (var handler in handlers)
                                {
                                    await handler(notification, cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessageProcessingError.Invoke(_logger, eventTypeName, ex);
                        }
#pragma warning restore CA1031 // Do not catch general exception types
                    }

                    // Complete the message
                    await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
                };

                // Handle errors
                processor.ProcessErrorAsync += args =>
                {
                    LogMessageProcessingFromSubscriptionError.Invoke(_logger, subscriptionName, args.Exception);
                    return Task.CompletedTask;
                };

                // Start processing
                await processor.StartProcessingAsync(cancellationToken).ConfigureAwait(false);
                _processors.Add(processor);
                LogStartOfProcessingMessages(_logger, subscriptionName, null);
            }

            // Keep the service running until cancellation is requested
            var tcs = new TaskCompletionSource();
            cancellationToken.Register(() => tcs.SetResult());
            await tcs.Task.ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var processor in _processors)
            {
                await processor.StopProcessingAsync().ConfigureAwait(false);
                await processor.DisposeAsync().ConfigureAwait(false);
            }

            await _sender.DisposeAsync().ConfigureAwait(false);
            await _client.DisposeAsync().ConfigureAwait(false);
        }
    }
}
