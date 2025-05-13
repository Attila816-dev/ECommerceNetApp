using ECommerceNetApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace ECommerceNetApp.Service.Implementation.EventBus
{
    internal sealed class InMemoryEventBus(InMemoryMessageQueue queue, ILogger<InMemoryEventBus> logger)
        : IEventBus, IAsyncDisposable
    {
        private static readonly Action<ILogger, string, string, Exception?> LogNotificationHandlerRegistration =
            LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, nameof(InMemoryEventBus)),
            "Handler {HandlerType} registered for notification {NotificationType}");

        private static readonly Action<ILogger, string, Exception?> LogEventPublished =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(InMemoryEventBus)),
            "Event {EventType} published to in-memory queue");

        private static readonly Action<ILogger, string, Exception?> LogStartOfProcessingMessages =
            LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(3, nameof(InMemoryEventBus)),
            "Started processing messages for event type {EventType}");

        private static readonly Action<ILogger, string, string, Exception?> LogMessageProcessingErrorInHandler =
            LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(5, nameof(InMemoryEventBus)),
            "Error in handler {HandlerType} processing message of type {EventType}");

        private static readonly Action<ILogger, string, Exception?> LogMessageProcessingError =
            LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(6, nameof(InMemoryEventBus)),
            "Unhandled error processing message of type {EventType}");

        private readonly object _handlersLock = new();
        private readonly Dictionary<Type, List<Func<INotification, CancellationToken, Task>>> _handlers = new();
        private readonly InMemoryMessageQueue _queue = queue;
        private readonly ILogger<InMemoryEventBus> _logger = logger;
        private bool _isStarted;
        private CancellationTokenSource? _cts;

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
        }

        public async Task PublishAsync<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : class, INotification
        {
            ArgumentNullException.ThrowIfNull(notification);

            await _queue.Writer.WriteAsync(notification, cancellationToken).ConfigureAwait(false);
            LogEventPublished(_logger, notification.GetType().Name, null);
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
        {
            if (_isStarted)
            {
                return;
            }

            _isStarted = true;

            // Create a linked token source that will be canceled when either the provided token is canceled or when DisposeAsync is called
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await foreach (var notification in _queue.Reader.ReadAllAsync(_cts.Token).ConfigureAwait(false))
            {
                var eventType = notification.GetType();
                LogStartOfProcessingMessages(_logger, eventType.Name, null);

                if (_handlers.TryGetValue(eventType, out var handlers))
                {
                    // Create a retry policy for event processing
                    var retryPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                    foreach (var handler in handlers)
                    {
                        await TryProcessMessageAsync(notification, eventType, retryPolicy, handler).ConfigureAwait(false);
                    }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_cts != null)
            {
                await _cts.CancelAsync().ConfigureAwait(false);
                _cts.Dispose();
                _cts = null;
            }

            _isStarted = false;

            // Complete the writer to signal no more items will be added
            _queue.Writer?.TryComplete();

            await Task.CompletedTask.ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        private async Task TryProcessMessageAsync(INotification notification, Type eventType, AsyncRetryPolicy retryPolicy, Func<INotification, CancellationToken, Task> handler)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                // Use retry policy for each handler
                await retryPolicy.ExecuteAsync(
                    async (ct) =>
                    {
                        try
                        {
                            await handler(notification, ct).ConfigureAwait(false);
                        }
                        catch (Exception handlerEx)
                        {
                            LogMessageProcessingErrorInHandler(
                                _logger,
                                handler.Method.DeclaringType?.Name ?? "Unknown",
                                eventType.Name,
                                handlerEx);
                            throw; // Rethrow to trigger retry
                        }
                    },
                    _cts?.Token ?? CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the error but continue processing other handlers
                LogMessageProcessingError(_logger, eventType.Name, ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
