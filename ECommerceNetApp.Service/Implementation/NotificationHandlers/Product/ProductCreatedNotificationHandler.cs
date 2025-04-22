using System.Text.Json;
using ECommerceNetApp.Domain.Events.Product;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductCreatedNotificationHandler(ILogger<ProductCreatedNotificationHandler> logger)
        : INotificationHandler<ProductCreatedEvent>
    {
        private readonly ILogger<ProductCreatedNotificationHandler> _logger = logger;

        public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogProductCreated(notification.ProductId, JsonSerializer.Serialize(notification));

            // Add any business logic that should happen when a Product is created
            // For example, you could send an email to administrators, update search indexes, etc.
            return Task.CompletedTask;
        }
    }
}
