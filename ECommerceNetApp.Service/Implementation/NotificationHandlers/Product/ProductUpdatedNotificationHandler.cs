using System.Text.Json;
using ECommerceNetApp.Domain.Events.Product;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductUpdatedNotificationHandler(ILogger<ProductUpdatedNotificationHandler> logger)
        : INotificationHandler<ProductUpdatedEvent>
    {
        private readonly ILogger<ProductUpdatedNotificationHandler> _logger = logger;

        public Task Handle(ProductUpdatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogProductUpdated(notification.ProductId, JsonSerializer.Serialize(notification));

            // Add any business logic that should happen when a Product is updated
            return Task.CompletedTask;
        }
    }
}
