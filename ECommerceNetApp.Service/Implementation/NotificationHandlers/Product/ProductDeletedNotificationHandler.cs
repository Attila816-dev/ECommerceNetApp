﻿using ECommerceNetApp.Domain.Events.Product;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductDeletedNotificationHandler(ILogger<ProductDeletedNotificationHandler> logger)
        : INotificationHandler<ProductDeletedEvent>
    {
        private static readonly Action<ILogger, int, Exception?> LogProductDeleted =
            LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(1, nameof(ProductDeletedNotificationHandler)),
                "Product deleted: {ProductId}.");

        private readonly ILogger<ProductDeletedNotificationHandler> _logger = logger;

        public Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            LogProductDeleted(_logger, notification.ProductId, null);

            // Add any business logic that should happen when a Product is deleted
            return Task.CompletedTask;
        }
    }
}
