using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.Options;
using Microsoft.Azure.Amqp.Framing;
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

        private static readonly Action<ILogger, string, Exception?> LogTopicCreated =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(AzureEventBus)), "Created topic {TopicName}");

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
            // First, ensure the topic exists
            if (!await _adminClient.TopicExistsAsync(_eventBusOptions.Value.TopicName, cancellationToken).ConfigureAwait(false))
            {
                await _adminClient.CreateTopicAsync(new CreateTopicOptions(_eventBusOptions.Value.TopicName), cancellationToken).ConfigureAwait(false);
                LogTopicCreated.Invoke(_logger, _eventBusOptions.Value.TopicName, null);
            }

            // For each event type we handle, ensure a subscription exists
            foreach (var eventType in _handlers.Keys)
            {
                var subscriptionName = $"{_eventBusOptions.Value.TopicName}-{eventType.Name}";

                await CreateSubscriptionIfNotExistAsync(eventType, subscriptionName, cancellationToken).ConfigureAwait(false);

                // Create a processor for this subscription
                var processor = _client.CreateProcessor(_eventBusOptions.Value.TopicName, subscriptionName);

                // Handle messages
                processor.ProcessMessageAsync += args => ProcessMessageAsync(args, eventType, cancellationToken);

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

        private async Task ProcessMessageAsync(ProcessMessageEventArgs eventArgs, Type eventType, CancellationToken cancellationToken)
        {
            var message = eventArgs.Message.Body.ToString();
            var eventTypeName = eventArgs.Message.Subject;
            var messageId = eventArgs.Message.MessageId;

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
                            try
                            {
                                await handler(notification, cancellationToken).ConfigureAwait(false);
                            }
                            catch (Exception)
                            {
                                await eventArgs.AbandonMessageAsync(eventArgs.Message, cancellationToken: cancellationToken).ConfigureAwait(false);
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Dead-letter the message as it's not properly formatted
                        await eventArgs.DeadLetterMessageAsync(eventArgs.Message, "DeserializationFailed", $"Could not deserialize as {eventType.Name}", cancellationToken).ConfigureAwait(false);
                    }

                    // Complete the message
                    await eventArgs.CompleteMessageAsync(eventArgs.Message, cancellationToken).ConfigureAwait(false);
                }
                catch (JsonException ex)
                {
                    // Dead-letter the message as it's not properly formatted
                    await eventArgs.DeadLetterMessageAsync(eventArgs.Message, "JsonDeserializationFailed", ex.Message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogMessageProcessingError.Invoke(_logger, eventTypeName, ex);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
            else
            {
                // No handlers for this type - complete it anyway
                await eventArgs.CompleteMessageAsync(eventArgs.Message, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task CreateSubscriptionIfNotExistAsync(Type eventType, string subscriptionName, CancellationToken cancellationToken)
        {
            if (!await _adminClient.SubscriptionExistsAsync(_eventBusOptions.Value.TopicName, subscriptionName, cancellationToken).ConfigureAwait(false))
            {
                // Create the subscription with a default rule that accepts all messages
                var subscriptionCreationOptions = new CreateSubscriptionOptions(_eventBusOptions.Value.TopicName, subscriptionName)
                {
                    DefaultMessageTimeToLive = TimeSpan.FromDays(_eventBusOptions.Value.DefaultMessageTimeToLiveInDays), // Set message expiration
                };

                // Create rule to filter by event type
                var rule = new CreateRuleOptions(
                    $"{eventType.Name}Rule",
                    new CorrelationRuleFilter { Subject = eventType.Name });

                await _adminClient.CreateSubscriptionAsync(subscriptionCreationOptions, rule, cancellationToken).ConfigureAwait(false);
                LogSubscripitionCreated.Invoke(_logger, subscriptionName, eventType.Name, null);
            }
        }
    }
}
