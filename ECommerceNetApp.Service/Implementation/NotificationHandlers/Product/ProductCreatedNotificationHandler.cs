﻿using System.Text.Json;
using ECommerceNetApp.Domain.Events.Product;
using ECommerceNetApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductCreatedNotificationHandler(ILogger<ProductCreatedNotificationHandler> logger)
        : INotificationHandler<ProductCreatedEvent>
    {
        // Define a static LoggerMessage delegate for improved performance and consistent templates
        private static readonly Action<ILogger, string, Exception?> LogProductCreated =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1, nameof(ProductCreatedNotificationHandler)),
                "Product created with Properties: {SerializedNotification}");

        private readonly ILogger<ProductCreatedNotificationHandler> _logger = logger;

        public void Register(IEventBus eventBus)
        {
            ArgumentNullException.ThrowIfNull(eventBus);
            eventBus.Register(this);
        }

        public Task HandleAsync(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            LogProductCreated(_logger, JsonSerializer.Serialize(notification), null);

            // Add any business logic that should happen when a Product is created
            // For example, you could send an email to administrators, update search indexes, etc.
            return Task.CompletedTask;
        }
    }
}
