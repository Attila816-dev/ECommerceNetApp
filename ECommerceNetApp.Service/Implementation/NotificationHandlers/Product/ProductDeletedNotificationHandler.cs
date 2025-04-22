using ECommerceNetApp.Domain.Events.Product;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductDeletedNotificationHandler(ILogger<ProductDeletedNotificationHandler> logger)
        : INotificationHandler<ProductDeletedEvent>
    {
        private readonly ILogger<ProductDeletedNotificationHandler> _logger = logger;

        public Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogProductDeleted(notification.ProductId);

            // Add any business logic that should happen when a Product is deleted
            return Task.CompletedTask;
        }
    }
}
