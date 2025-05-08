using System.Text.Json;
using ECommerceNetApp.Domain.Events.Product;
using ECommerceNetApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductUpdatedNotificationHandler(ILogger<ProductUpdatedNotificationHandler> logger)
        : INotificationHandler<ProductUpdatedEvent>
    {
        // Define a static LoggerMessage delegate for improved performance and consistent templates
        private static readonly Action<ILogger, int, string, Exception?> LogProductCreated =
            LoggerMessage.Define<int, string>(
                LogLevel.Information,
                new EventId(1, nameof(ProductUpdatedNotificationHandler)),
                "Product {ProductId} updated with properties: {SerializedNotification}");

        private readonly ILogger<ProductUpdatedNotificationHandler> _logger = logger;

        public void Register(IEventBus eventBus)
        {
            ArgumentNullException.ThrowIfNull(eventBus, nameof(eventBus));
            eventBus.Register(this);
        }

        public Task HandleAsync(ProductUpdatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);

            LogProductCreated(
                _logger,
                notification.ProductId,
                JsonSerializer.Serialize(notification),
                null);

            // Add any business logic that should happen when a Product is updated
            return Task.CompletedTask;
        }
    }
}
