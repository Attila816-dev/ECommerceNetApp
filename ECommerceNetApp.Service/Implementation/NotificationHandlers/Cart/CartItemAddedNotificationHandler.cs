using System.Text.Json;
using ECommerceNetApp.Domain.Events.Cart;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Cart
{
    public class CartItemAddedNotificationHandler(ILogger<CartItemAddedNotificationHandler> logger)
        : INotificationHandler<CartItemAddedEvent>
    {
        private readonly ILogger<CartItemAddedNotificationHandler> _logger = logger;

        public Task Handle(CartItemAddedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogCartItemAdded(notification.CartId, JsonSerializer.Serialize(notification));

            // Add any business logic that should happen when a cart item is created
            // For example, you could send an email to administrators, update search indexes, etc.
            return Task.CompletedTask;
        }
    }
}
