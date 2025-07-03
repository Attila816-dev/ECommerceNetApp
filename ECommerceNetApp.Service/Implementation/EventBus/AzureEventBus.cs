using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

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
            new EventId(2, nameof(AzureEventBus)),
            "Event {EventType} published to Azure Service Bus");

        private static readonly Action<ILogger, string, string, Exception?> LogSubscripitionCreated =
            LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(3, nameof(AzureEventBus)),
            "Created subscription {SubscriptionName} for {EventType}");

        private static readonly Action<ILogger, string, Exception?> LogTopicCreated =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(AzureEventBus)), "Created topic {TopicName}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageReceived =
            LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(4, nameof(AzureEventBus)),
            "Received message: {MessageBody} of type {EventType}");

        private static readonly Action<ILogger, string, Exception?> LogStartOfProcessingMessages =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(5, nameof(AzureEventBus)),
            "Started processing messages for subscription {SubscriptionName}");

        private static readonly Action<ILogger, string, Exception?> LogCompletionOfProcessingMessages =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6, nameof(AzureEventBus)),
            "Completed processing message {MessageId}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageProcessingError =
            LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(7, nameof(AzureEventBus)),
            "Unhandled error processing message {MessageId} of type {EventType}");

        private static readonly Action<ILogger, string, Exception?> LogMessageProcessingFromSubscriptionError =
            LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(8, nameof(AzureEventBus)),
            "Error processing messages from subscription {Subscription}");

        private static readonly Action<ILogger, string, string, string, Exception?> LogMessageProcessingErrorInHandler =
            LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(9, nameof(AzureEventBus)),
            "Error in handler {HandlerType} processing message {MessageId} of type {EventType}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageDeserializationError =
            LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(10, nameof(AzureEventBus)),
            "JSON deserialization error for message {MessageId} of type {EventType}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageDeserializationMismatchError =
            LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(11, nameof(AzureEventBus)),
            "Could not deserialize message {MessageId} as {EventType}");

        private static readonly Action<ILogger, string, string, Exception?> LogMissingMessageProcessingHandler =
            LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(12, nameof(AzureEventBus)),
            "No handlers registered for event type {EventType}, completing message {MessageId}");

        private static readonly Action<ILogger, Exception?> LogEventBusAlreadyStarted =
            LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(13, nameof(AzureEventBus)),
            "Azure Event Bus is already started");

        private static readonly Action<ILogger, string, Exception?> LogErrorAtSettingUpRules =
            LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(14, nameof(AzureEventBus)),
            "Error setting up rules for subscription {SubscriptionName}");

        private static readonly Action<ILogger, string, string, Exception?> LogCreatedSubscriptionFilterRule =
            LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(15, nameof(AzureEventBus)),
            "Created filter rule for {EventType} on subscription {SubscriptionName}");

        private readonly object _handlersLock = new();
        private readonly AzureServiceBusFactory _serviceBusFactory;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<AzureEventBus> _logger;
        private readonly Dictionary<Type, List<Func<INotification, CancellationToken, Task>>> _handlers = new();
        private readonly List<ServiceBusProcessor> _processors = new();
        private readonly string _topicName;
        private readonly bool _autoCreateEntities;
        private readonly TimeSpan _defaultMessageTimeToLive;
        private bool _isStarted;

        public AzureEventBus(
            AzureServiceBusFactory serviceBusFactory,
            IOptions<EventBusOptions> eventBusOptions,
            ILogger<AzureEventBus> logger)
        {
            _serviceBusFactory = serviceBusFactory;
            _logger = logger;
            _adminClient = _serviceBusFactory.CreateAdministrationClient();
            _sender = _serviceBusFactory.CreateSender();
            _topicName = eventBusOptions.Value.AzureOptions!.TopicName;
            _autoCreateEntities = eventBusOptions.Value.AutoCreateEntities;
            _defaultMessageTimeToLive = TimeSpan.FromDays(eventBusOptions.Value.DefaultMessageTimeToLiveInDays); // Set message expiration
        }

        public void Register<TNotification>(INotificationHandler<TNotification> handler)
            where TNotification : INotification
        {
            var eventType = typeof(TNotification);

            lock (_handlersLock)
            {
                if (!_handlers.TryGetValue(eventType, out var handlerList))
                {
                    handlerList = new();
                    _handlers[eventType] = handlerList;
                }

                handlerList.Add((evt, token) => handler.HandleAsync((TNotification)evt, token));
                LogNotificationHandlerRegistration(_logger, handler.GetType().Name, typeof(TNotification).Name, null);
            }

            // If we've already started consuming, we need to add a processor for this new handler
            if (_isStarted)
            {
                _ = SetupSubscriptionAndProcessorAsync(eventType, default);
            }
        }

        public async Task PublishAsync<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : class, INotification
        {
            var eventTypeName = notification.GetType().Name;
            var messageBody = JsonSerializer.Serialize(notification);
            var message = new ServiceBusMessage()
            {
                Subject = eventTypeName,
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                Body = new BinaryData(Encoding.UTF8.GetBytes(messageBody)),
            };

            await _sender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
            LogEventPublishedToAzureServiceBus(_logger, eventTypeName, null);
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
        {
            if (_isStarted)
            {
                LogEventBusAlreadyStarted(_logger, null);
                return;
            }

            // First, ensure the topic exists
            if (_autoCreateEntities && !await _adminClient.TopicExistsAsync(_topicName, cancellationToken).ConfigureAwait(false))
            {
                await _adminClient.CreateTopicAsync(new CreateTopicOptions(_topicName), cancellationToken).ConfigureAwait(false);
                LogTopicCreated(_logger, _topicName!, null);
            }

            // For each event type we handle, ensure a subscription exists
            foreach (var eventType in _handlers.Keys)
            {
                await SetupSubscriptionAndProcessorAsync(eventType, cancellationToken).ConfigureAwait(false);
            }

            // Keep the service running until cancellation is requested
            _isStarted = true;
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var processor in _processors)
            {
                await processor.StopProcessingAsync().ConfigureAwait(false);
                await processor.DisposeAsync().ConfigureAwait(false);
            }

            _processors.Clear();
            await _sender.DisposeAsync().ConfigureAwait(false);
            await _serviceBusFactory.DisposeAsync().ConfigureAwait(false);
        }

        private async Task SetupSubscriptionAndProcessorAsync(Type eventType, CancellationToken cancellationToken)
        {
            await CreateSubscriptionIfNotExistAsync(eventType, cancellationToken).ConfigureAwait(false);

            // Create a processor for this subscription
            var subscriptionName = GetSubscriptionName(eventType);
            var processor = _serviceBusFactory.CreateProcessor(subscriptionName);

            // Handle messages
            processor.ProcessMessageAsync += async eventArgs => await TryProcessMessageAsync(eventArgs, eventType, cancellationToken).ConfigureAwait(false);

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

        private async Task TryProcessMessageAsync(ProcessMessageEventArgs eventArgs, Type eventType, CancellationToken cancellationToken)
        {
            var message = eventArgs.Message.Body.ToString();
            var eventTypeName = eventArgs.Message.Subject;
            var messageId = eventArgs.Message.MessageId;

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            LogMessageReceived(_logger, message, eventTypeName, null);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                try
                {
                    await retryPolicy.ExecuteAsync(
                        (ct) => ProcessMessageAsync(eventArgs, eventType, handlers, ct),
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogMessageProcessingError(_logger, messageId, eventTypeName, ex);
                    await eventArgs.DeadLetterMessageAsync(eventArgs.Message, "ProcessingFailed", "The message could not be processed after retries.", cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                // No handlers for this type - complete it anyway
                LogMissingMessageProcessingHandler(_logger, eventTypeName, messageId, null);
                await eventArgs.CompleteMessageAsync(eventArgs.Message, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs eventArgs, Type eventType, List<Func<INotification, CancellationToken, Task>> handlers, CancellationToken cancellationToken)
        {
            var message = eventArgs.Message.Body.ToString();
            var eventTypeName = eventArgs.Message.Subject;
            var messageId = eventArgs.Message.MessageId;

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
                        catch (Exception handlerEx)
                        {
                            LogMessageProcessingErrorInHandler(_logger, handler.Method.DeclaringType?.Name ?? "Unknown", messageId, eventTypeName, handlerEx);
                            await eventArgs.AbandonMessageAsync(eventArgs.Message, cancellationToken: cancellationToken).ConfigureAwait(false);
                            return;
                        }
                    }

                    // Complete the message since all handlers executed (even if some had errors)
                    LogCompletionOfProcessingMessages(_logger, messageId, null);
                    await eventArgs.CompleteMessageAsync(eventArgs.Message, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Dead-letter the message as it's not properly formatted
                    LogMessageDeserializationMismatchError(_logger, messageId, eventTypeName, null);
                    await eventArgs.DeadLetterMessageAsync(eventArgs.Message, "DeserializationFailed", $"Could not deserialize as {eventType.Name}", cancellationToken).ConfigureAwait(false);
                }
            }
            catch (JsonException ex)
            {
                // Dead-letter the message as it's not properly formatted
                LogMessageDeserializationError(_logger, messageId, eventTypeName, ex);
                await eventArgs.DeadLetterMessageAsync(eventArgs.Message, "JsonDeserializationFailed", ex.Message, cancellationToken).ConfigureAwait(false);
            }
            catch (ServiceBusException ex)
            {
                LogMessageDeserializationError(_logger, messageId, eventTypeName, ex);
                await eventArgs.DeadLetterMessageAsync(eventArgs.Message, ex.Reason.ToString(), ex.Message, cancellationToken).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                LogMessageProcessingError(_logger, messageId, eventTypeName, ex);
                await eventArgs.AbandonMessageAsync(eventArgs.Message, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task CreateSubscriptionIfNotExistAsync(Type eventType, CancellationToken cancellationToken)
        {
            var subscriptionName = GetSubscriptionName(eventType);
            if (!_autoCreateEntities || await _adminClient.SubscriptionExistsAsync(_topicName, subscriptionName, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            await _adminClient.CreateSubscriptionAsync(GetSubscriptionCreationOptions(eventType), cancellationToken).ConfigureAwait(false);
            LogSubscripitionCreated(_logger, subscriptionName, eventType.Name, null);

            try
            {
                // Delete the default rule
                await _adminClient.DeleteRuleAsync(_topicName, subscriptionName, "$Default", cancellationToken).ConfigureAwait(false);

                // Create rule to filter by event type
                var rule = new CreateRuleOptions(
                    $"{eventType.Name}Rule",
                    new CorrelationRuleFilter { Subject = eventType.Name });

                await _adminClient.CreateRuleAsync(_topicName, subscriptionName, rule, cancellationToken).ConfigureAwait(false);
                LogCreatedSubscriptionFilterRule(_logger, eventType.Name, subscriptionName, null);
            }
            catch (Exception ex)
            {
                // Continue anyway - the subscription exists, but filtering might not work as expected
                LogErrorAtSettingUpRules(_logger, subscriptionName, ex);
                throw;
            }
        }

        private string GetSubscriptionName(Type eventType) => $"{_topicName}-{eventType.Name}";

        private CreateSubscriptionOptions GetSubscriptionCreationOptions(Type eventType)
            => new CreateSubscriptionOptions(_topicName, GetSubscriptionName(eventType))
            {
                DefaultMessageTimeToLive = _defaultMessageTimeToLive,
            };
    }
}
