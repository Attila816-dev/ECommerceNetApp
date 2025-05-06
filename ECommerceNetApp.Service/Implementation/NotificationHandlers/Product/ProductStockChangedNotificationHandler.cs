using ECommerceNetApp.Domain.Events.Product;
using ECommerceNetApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductStockChangedNotificationHandler : INotificationHandler<ProductStockChangedEvent>
    {
        private static readonly Action<ILogger, int, int, int, Exception?> LogProductStockChanged =
            LoggerMessage.Define<int, int, int>(
                LogLevel.Information,
                new EventId(1, nameof(ProductStockChangedNotificationHandler)),
                "Product stock changed: {ProductId}, Old: {OldAmount}, New: {NewAmount}");

        private readonly ILogger<ProductStockChangedNotificationHandler> _logger;

        public ProductStockChangedNotificationHandler(ILogger<ProductStockChangedNotificationHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(ProductStockChangedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);

            LogProductStockChanged(_logger, notification.ProductId, notification.OldAmount, notification.NewAmount, null);

            // Add any business logic that should happen when a product's stock changes
            // For example, you could send notifications if stock is low
            return Task.CompletedTask;
        }
    }
}
