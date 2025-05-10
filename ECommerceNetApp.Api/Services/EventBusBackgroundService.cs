using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Api.Services
{
    public class EventBusBackgroundService(IEventBus eventBus, IServiceProvider serviceProvider, ILogger<EventBusBackgroundService> logger)
            : BackgroundService
    {
        private static readonly Action<ILogger, Exception?> LogEventBusBackgroundServiceStopped =
            LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, nameof(EventBusBackgroundService)),
            "Event bus consumer stopped due to application shutdown");

        private static readonly Action<ILogger, Exception?> LogEventBusBackgroundServiceError =
            LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(EventBusBackgroundService)),
            "Error occurred while running the event bus consumer");

        private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        private readonly ILogger<EventBusBackgroundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_eventBus is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }

            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            // Step 1: Find all types implementing INotification
            var notificationsAssembly = typeof(INotification).Assembly;
            var notificationTypes = notificationsAssembly
                .GetTypes()
                .Where(t => typeof(INotification).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            // Step 2: For each INotification type, find and resolve handlers
            foreach (var notificationType in notificationTypes)
            {
                var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

                var handlers = scope.ServiceProvider.GetServices(handlerType);

                foreach (var handler in handlers.OfType<INotificationHandler>())
                {
                    // Optionally invoke Register or HandleAsync via reflection
                    handler.Register(_eventBus);
                }
            }

            try
            {
                await _eventBus.StartConsumingAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // This is expected during shutdown, don't log an error
                LogEventBusBackgroundServiceStopped(_logger, null);
            }
            catch (Exception ex)
            {
                LogEventBusBackgroundServiceError(_logger, ex);
                throw;
            }
        }
    }
}
