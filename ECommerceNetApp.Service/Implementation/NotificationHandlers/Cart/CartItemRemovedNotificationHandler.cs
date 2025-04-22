using ECommerceNetApp.Domain.Events.Cart;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Cart
{
    public class CartItemRemovedNotificationHandler(ILogger<CartItemRemovedNotificationHandler> logger)
        : INotificationHandler<CartItemRemovedEvent>
    {
        private readonly ILogger<CartItemRemovedNotificationHandler> _logger = logger;

        public Task Handle(CartItemRemovedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogCartItemRemoved(notification.CartId, notification.CartItemId);

            // Add any business logic that should happen when a category is updated
            return Task.CompletedTask;
        }
    }
}
