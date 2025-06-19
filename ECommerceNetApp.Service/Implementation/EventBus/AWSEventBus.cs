using System.Globalization;
using System.Text.Json;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using SNSMessageAttributeValue = Amazon.SimpleNotificationService.Model.MessageAttributeValue;

namespace ECommerceNetApp.Service.Implementation.EventBus
{
    /// <summary>
    /// Implementation of IEventBus using AWS SNS (Simple Notification Service) for publishing
    /// and SQS (Simple Queue Service) for consuming messages.
    /// </summary>
    internal sealed partial class AwsEventBus : IEventBus, IAsyncDisposable
    {
        private static readonly Action<ILogger, string, string, Exception?> LogNotificationHandlerRegistration =
            LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, nameof(AwsEventBus)),
            "Handler {HandlerType} registered for notification {NotificationType}");

        private static readonly Action<ILogger, string, Exception?> LogEventPublishedToSNS =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(AwsEventBus)),
            "Event {EventType} published to AWS SNS");

        private static readonly Action<ILogger, string, string, Exception?> LogSubscriptionCreated =
            LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(3, nameof(AwsEventBus)),
            "Created subscription {SubscriptionName} for {EventType}");

        private static readonly Action<ILogger, string, Exception?> LogTopicCreated =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(4, nameof(AwsEventBus)),
            "Created topic {TopicName}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageReceived =
            LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(5, nameof(AwsEventBus)),
            "Received message: {MessageBody} of type {EventType}");

        private static readonly Action<ILogger, string, Exception?> LogStartOfProcessingMessages =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6, nameof(AwsEventBus)),
            "Started processing messages for queue {QueueName}");

        private static readonly Action<ILogger, string, Exception?> LogCompletionOfProcessingMessages =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(7, nameof(AwsEventBus)),
            "Completed processing message {MessageId}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageProcessingError =
            LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(8, nameof(AwsEventBus)),
            "Unhandled error processing message {MessageId} of type {EventType}");

        private static readonly Action<ILogger, string, Exception?> LogMessageProcessingFromQueueError =
            LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(9, nameof(AwsEventBus)),
            "Error processing messages from queue {QueueName}");

        private static readonly Action<ILogger, string, string, string, Exception?> LogMessageProcessingErrorInHandler =
            LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(10, nameof(AwsEventBus)),
            "Error in handler {HandlerType} processing message {MessageId} of type {EventType}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageDeserializationError =
            LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(11, nameof(AwsEventBus)),
            "JSON deserialization error for message {MessageId} of type {EventType}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageDeserializationMismatchError =
            LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(12, nameof(AwsEventBus)),
            "Could not deserialize message {MessageId} as {EventType}");

        private static readonly Action<ILogger, string, string, Exception?> LogMissingMessageProcessingHandler =
            LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(13, nameof(AwsEventBus)),
            "No handlers registered for event type {EventType}, completing message {MessageId}");

        private static readonly Action<ILogger, Exception?> LogEventBusAlreadyStarted =
            LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(14, nameof(AwsEventBus)),
            "AWS Event Bus is already started");

        private readonly object _handlersLock = new();
        private readonly AWSEventBusFactory _factory;
        private readonly ILogger<AwsEventBus> _logger;
        private readonly Dictionary<Type, List<Func<INotification, CancellationToken, Task>>> _handlers = new();
        private readonly Dictionary<Type, CancellationTokenSource> _pollingTokenSources = new();
        private readonly Dictionary<Type, Task> _pollingTasks = new();
        private readonly string _topicName;
        private readonly bool _autoCreateEntities;
        private readonly TimeSpan _defaultMessageTimeToLive;
        private readonly string? _deadLetterQueueArn;
        private CancellationTokenSource? _masterCancellationTokenSource;
        private bool _isStarted;

        public AwsEventBus(
            AWSEventBusFactory factory,
            IOptions<EventBusOptions> eventBusOptions,
            ILogger<AwsEventBus> logger)
        {
            _factory = factory;
            _logger = logger;
            _topicName = eventBusOptions.Value.AWSOptions!.TopicName;
            _autoCreateEntities = eventBusOptions.Value.AutoCreateEntities;
            _deadLetterQueueArn = eventBusOptions.Value.AWSOptions!.DeadLetterQueueArn;
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
                _ = SetupQueueAndPollingForEventTypeAsync(eventType, _masterCancellationTokenSource!.Token);
            }
        }

        public async Task PublishAsync<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : class, INotification
        {
            ArgumentNullException.ThrowIfNull(notification);

            var eventTypeName = notification.GetType().Name;
            var messageBody = JsonSerializer.Serialize(notification);
            var topicArn = await EnsureTopicExistsAsync(cancellationToken).ConfigureAwait(false);

            var snsClient = _factory.CreateSnsClient();
            var request = new PublishRequest
            {
                TopicArn = topicArn,
                Message = messageBody,
                MessageAttributes = new Dictionary<string, SNSMessageAttributeValue>
                {
                    {
                        "EventType", new SNSMessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = eventTypeName,
                        }
                    },
                },
            };

            var response = await snsClient.PublishAsync(request, cancellationToken).ConfigureAwait(false);
            LogEventPublishedToSNS(_logger, eventTypeName, null);
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
        {
            if (_isStarted)
            {
                LogEventBusAlreadyStarted(_logger, null);
                return;
            }

            // Create a master cancellation token source that combines the provided token
            // This will be used to stop all consumers when the service is shutting down
            _masterCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Make sure the SNS topic exists
            await EnsureTopicExistsAsync(_masterCancellationTokenSource.Token).ConfigureAwait(false);

            // For each registered event type, set up a dedicated SQS queue and subscription
            foreach (var eventType in _handlers.Keys)
            {
                await SetupQueueAndPollingForEventTypeAsync(eventType, _masterCancellationTokenSource.Token).ConfigureAwait(false);
            }

            _isStarted = true;

            // Wait indefinitely until cancellation is requested
            await Task.Delay(Timeout.Infinite, _masterCancellationTokenSource.Token).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_masterCancellationTokenSource != null)
            {
                await _masterCancellationTokenSource.CancelAsync().ConfigureAwait(false);
                _masterCancellationTokenSource.Dispose();
                _masterCancellationTokenSource = null;
            }

            // Cancel all individual polling tasks
            foreach (var cts in _pollingTokenSources.Values)
            {
                await cts.CancelAsync().ConfigureAwait(false);
                cts.Dispose();
            }

            _pollingTokenSources.Clear();

            // Wait for all polling tasks to complete
            if (_pollingTasks.Count > 0)
            {
                await Task.WhenAll(_pollingTasks.Values).ConfigureAwait(false);
            }

            _pollingTasks.Clear();

            _isStarted = false;

            // Suppress finalizers
            GC.SuppressFinalize(this);
        }

        private async Task<string> EnsureTopicExistsAsync(CancellationToken cancellationToken)
        {
            var snsClient = _factory.CreateSnsClient();
            string topicArn;

            if (_autoCreateEntities)
            {
                // Check if topic exists by listing topics and filtering
                var topics = await snsClient.ListTopicsAsync(cancellationToken).ConfigureAwait(false);
                var existingTopic = topics.Topics
                    .FirstOrDefault(t => t.TopicArn.EndsWith($":{_topicName}", StringComparison.OrdinalIgnoreCase));

                if (existingTopic == null)
                {
                    // Create the topic if it doesn't exist
                    var createTopicRequest = new CreateTopicRequest(_topicName);
                    var createTopicResponse = await snsClient.CreateTopicAsync(createTopicRequest, cancellationToken).ConfigureAwait(false);
                    topicArn = createTopicResponse.TopicArn;
                    LogTopicCreated(_logger, _topicName, null);
                }
                else
                {
                    topicArn = existingTopic.TopicArn;
                }
            }
            else
            {
                // Find the topic by name, assuming it already exists
                var topics = await snsClient.ListTopicsAsync(cancellationToken).ConfigureAwait(false);
                var existingTopic = topics.Topics
                    .FirstOrDefault(t => t.TopicArn.EndsWith($":{_topicName}", StringComparison.OrdinalIgnoreCase));

                if (existingTopic == null)
                {
                    throw new InvalidOperationException($"Topic '{_topicName}' does not exist and auto-creation is disabled");
                }

                topicArn = existingTopic.TopicArn;
            }

            return topicArn;
        }

        private async Task SetupQueueAndPollingForEventTypeAsync(Type eventType, CancellationToken cancellationToken)
        {
            // Create unique queue name for this event type
            var queueName = $"{_topicName}-{eventType.Name}";

            // Ensure the queue exists and is subscribed to the topic
            var queueUrl = await EnsureQueueExistsAsync(queueName, eventType.Name, cancellationToken).ConfigureAwait(false);

            // Set up a cancellation token source for this specific queue/event type
            var pollingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _pollingTokenSources[eventType] = pollingCts;

            // Start a background task to poll for messages
            var pollingTask = StartPollingQueueAsync(queueUrl, eventType, pollingCts.Token);
            _pollingTasks[eventType] = pollingTask;
        }

        private async Task<string> EnsureQueueExistsAsync(string queueName, string eventTypeName, CancellationToken cancellationToken)
        {
            var sqsClient = _factory.CreateSqsClient();
            var snsClient = _factory.CreateSnsClient();
            string queueUrl;

            // First check if the queue exists
            try
            {
                var getQueueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName, cancellationToken).ConfigureAwait(false);
                queueUrl = getQueueUrlResponse.QueueUrl;
            }
            catch (QueueDoesNotExistException)
            {
                if (!_autoCreateEntities)
                {
                    throw new InvalidOperationException($"Queue '{queueName}' does not exist and auto-creation is disabled");
                }

                if (string.IsNullOrEmpty(_deadLetterQueueArn))
                {
                    throw new InvalidOperationException("Dead letter queue ARN is not configured");
                }

                // Create the queue
                var createQueueRequest = new CreateQueueRequest
                {
                    QueueName = queueName,
                    Attributes = new Dictionary<string, string>
                    {
                        // Set message retention period (in seconds)
                        { "MessageRetentionPeriod", _defaultMessageTimeToLive.TotalSeconds.ToString(CultureInfo.InvariantCulture) },

                        // Enable dead letter queue after 3 failures
                        { "RedrivePolicy", $"{{\"maxReceiveCount\":\"3\",\"deadLetterTargetArn\":\"{_deadLetterQueueArn}\"}}" },
                    },
                };

                // First create the queue
                var createQueueResponse = await sqsClient.CreateQueueAsync(createQueueRequest, cancellationToken).ConfigureAwait(false);
                queueUrl = createQueueResponse.QueueUrl;

                // Create a dead letter queue
                var deadLetterQueueName = $"{queueName}-DLQ";
                var createDlqRequest = new CreateQueueRequest
                {
                    QueueName = deadLetterQueueName,
                    Attributes = new Dictionary<string, string>
                    {
                        // Retain messages for 14 days in the DLQ
                        { "MessageRetentionPeriod", "1209600" },
                    },
                };
                var createDlqResponse = await sqsClient.CreateQueueAsync(createDlqRequest, cancellationToken).ConfigureAwait(false);

                // Get the DLQ ARN
                var getDlqAttributesResponse = await sqsClient.GetQueueAttributesAsync(
                    createDlqResponse.QueueUrl,
                    new List<string> { "QueueArn" },
                    cancellationToken).ConfigureAwait(false);
                var dlqArn = getDlqAttributesResponse.QueueARN;

                // Update the main queue with the correct DLQ ARN
                var redrivePolicy = $"{{\"maxReceiveCount\":\"3\",\"deadLetterTargetArn\":\"{dlqArn}\"}}";
                await sqsClient.SetQueueAttributesAsync(
                    queueUrl,
                    new Dictionary<string, string> { { "RedrivePolicy", redrivePolicy } },
                    cancellationToken).ConfigureAwait(false);
            }

            // Get the queue ARN
            var getQueueAttributesResponse = await sqsClient.GetQueueAttributesAsync(
                queueUrl,
                new List<string> { "QueueArn" },
                cancellationToken).ConfigureAwait(false);
            var queueArn = getQueueAttributesResponse.QueueARN;

            // Get the SNS topic ARN
            var topicArn = await EnsureTopicExistsAsync(cancellationToken).ConfigureAwait(false);

            // Create a policy that allows the SNS topic to send messages to this queue
            var policy = @"{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {
                        ""Effect"": ""Allow"",
                        ""Principal"": { ""Service"": ""sns.amazonaws.com"" },
                        ""Action"": ""sqs:SendMessage"",
                        ""Resource"": """ + queueArn + @""",
                        ""Condition"": {
                            ""ArnEquals"": { ""aws:SourceArn"": """ + topicArn + @""" }
                        }
                    }
                ]
            }";

            // Set the queue policy
            await sqsClient.SetQueueAttributesAsync(
                queueUrl,
                new Dictionary<string, string> { { "Policy", policy } },
                cancellationToken).ConfigureAwait(false);

            // Check if this queue is already subscribed to the topic with the right filter
            var listSubscriptionsResponse = await snsClient.ListSubscriptionsByTopicAsync(topicArn, cancellationToken).ConfigureAwait(false);
            var existingSubscription = listSubscriptionsResponse.Subscriptions
                .FirstOrDefault(s => s.Endpoint == queueArn);

            if (existingSubscription == null)
            {
                // Create a subscription with a filter policy for this event type
                var subscribeRequest = new SubscribeRequest
                {
                    TopicArn = topicArn,
                    Protocol = "sqs",
                    Endpoint = queueArn,
                    Attributes = new Dictionary<string, string>
                    {
                        { "FilterPolicy", $"{{\"EventType\":[\"{eventTypeName}\"]}}" },
                    },
                };

                await snsClient.SubscribeAsync(subscribeRequest, cancellationToken).ConfigureAwait(false);
                LogSubscriptionCreated(_logger, queueName, eventTypeName, null);
            }

            return queueUrl;
        }

        private async Task StartPollingQueueAsync(string queueUrl, Type eventType, CancellationToken cancellationToken)
        {
            var sqsClient = _factory.CreateSqsClient();
            LogStartOfProcessingMessages(_logger, queueUrl, null);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Configure the receive message request
                        var receiveMessageRequest = new ReceiveMessageRequest
                        {
                            QueueUrl = queueUrl,
                            MaxNumberOfMessages = 10, // Process up to 10 messages at once
                            WaitTimeSeconds = 20, // Long polling (wait up to 20 seconds for messages)
                            MessageAttributeNames = new List<string> { "All" },
                            MessageSystemAttributeNames = new List<string> { "All" },
                        };

                        // Receive messages from the queue
                        var receiveMessageResponse = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, cancellationToken).ConfigureAwait(false);

                        // Process each message
                        foreach (var message in receiveMessageResponse.Messages)
                        {
                            await ProcessMessageAsync(sqsClient, queueUrl, message, eventType, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        // This is expected when cancellation is requested
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue polling
                        LogMessageProcessingFromQueueError(_logger, queueUrl, ex);

                        // Brief delay before retrying to avoid hammering the service
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // This is expected when cancellation is requested
            }
        }

        private async Task ProcessMessageAsync(AmazonSQSClient sqsClient, string queueUrl, Message message, Type eventType, CancellationToken cancellationToken)
        {
            string? eventTypeName = null;

            if (message.MessageAttributes.TryGetValue("EventType", out var eventTypeAttribute))
            {
                eventTypeName = eventTypeAttribute.StringValue;
            }

            LogMessageReceived(_logger, message.Body, eventTypeName ?? "Unknown", null);

            // Create a retry policy for processing
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            try
            {
                await retryPolicy.ExecuteAsync(
                    async (ct) =>
                    {
                        try
                        {
                            await TryProcessMessageAsync(sqsClient, queueUrl, message, eventType, eventTypeName, ct).ConfigureAwait(false);
                        }
                        catch (JsonException ex)
                        {
                            LogMessageDeserializationError(_logger, message.MessageId, eventTypeName ?? eventType.Name, ex);
                            await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, ct).ConfigureAwait(false);
                        }
                    },
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogMessageProcessingError(_logger, message.MessageId, eventTypeName ?? eventType.Name, ex);

                // Don't delete the message - it will become visible again after the visibility timeout expires
                // After multiple failures, the redrive policy will move it to the DLQ
            }
        }

        private async Task TryProcessMessageAsync(AmazonSQSClient sqsClient, string queueUrl, Message message, Type eventType, string? eventTypeName, CancellationToken cancellationToken)
        {
            var messageId = message.MessageId;
            var messageBody = message.Body;

            if (!_handlers.TryGetValue(eventType, out var handlers))
            {
                // No handlers for this type - delete the message anyway
                LogMissingMessageProcessingHandler(_logger, eventTypeName ?? eventType.Name, message.MessageId, null);
                await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken).ConfigureAwait(false);
                return;
            }

            // Check if this is an SNS notification wrapped in SQS message
            try
            {
                // SNS messages through SQS are wrapped in a JSON structure
                var snsWrapper = JsonSerializer.Deserialize<SNSMessageWrapper>(messageBody);
                if (snsWrapper != null && !string.IsNullOrEmpty(snsWrapper.Message))
                {
                    messageBody = snsWrapper.Message;
                }
            }
            catch
            {
                // If it's not an SNS wrapper, just use the message as is
            }

            var notification = JsonSerializer.Deserialize(messageBody, eventType) as INotification;
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
                        LogMessageProcessingErrorInHandler(
                            _logger,
                            handler.Method.DeclaringType?.Name ?? "Unknown",
                            messageId,
                            eventTypeName ?? eventType.Name,
                            handlerEx);

                        throw; // Rethrow to trigger retry policy
                    }
                }

                // Successfully processed - delete the message from the queue
                await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken).ConfigureAwait(false);
                LogCompletionOfProcessingMessages(_logger, messageId, null);
            }
            else
            {
                // Couldn't deserialize to the expected type
                LogMessageDeserializationMismatchError(_logger, messageId, eventType.Name, null);
                await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
