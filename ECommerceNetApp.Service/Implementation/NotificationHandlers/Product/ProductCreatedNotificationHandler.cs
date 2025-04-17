using System.Text.Json;
using ECommerceNetApp.Domain.Events.Product;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductCreatedNotificationHandler(ILogger<ProductCreatedNotificationHandler> logger)
        : INotificationHandler<ProductCreatedEvent>
    {
        // Define a static LoggerMessage delegate for improved performance and consistent templates
        private static readonly Action<ILogger, int, string, Exception?> LogProductCreated =
            LoggerMessage.Define<int, string>(
                LogLevel.Information,
                new EventId(1, nameof(ProductCreatedNotificationHandler)),
                "Product {ProductId} created with Properties: {SerializedNotification}");

        private readonly ILogger<ProductCreatedNotificationHandler> _logger = logger;

        public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);

            LogProductCreated(
                _logger,
                notification.ProductId,
                JsonSerializer.Serialize(notification),
                null);

            // Add any business logic that should happen when a Product is created
            // For example, you could send an email to administrators, update search indexes, etc.
            return Task.CompletedTask;
        }
    }
}
