using ECommerceNetApp.Domain.Events.Product;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductStockChangedNotificationHandler : INotificationHandler<ProductStockChangedEvent>
    {
        private readonly ILogger<ProductStockChangedNotificationHandler> _logger;

        public ProductStockChangedNotificationHandler(ILogger<ProductStockChangedNotificationHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductStockChangedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogProductStockChanged(notification.ProductId, notification.OldAmount, notification.NewAmount);

            // Add any business logic that should happen when a product's stock changes
            // For example, you could send notifications if stock is low
            return Task.CompletedTask;
        }
    }
}
