using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Api.Services
{
    public class EventBusBackgroundService(IEventBus eventBus, IServiceProvider serviceProvider)
            : BackgroundService
    {
        private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

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

                foreach (var handler in handlers)
                {
                    // Optionally invoke Register or HandleAsync via reflection
                    var registerMethod = handlerType.GetMethod("Register");
                    registerMethod?.Invoke(handler, new object[] { eventBus });
                }
            }

            await _eventBus.StartConsumingAsync(stoppingToken).ConfigureAwait(false);
        }
    }
}
