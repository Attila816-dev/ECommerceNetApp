using ECommerceNetApp.Domain.Events.Cart;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Cart
{
    public class CartDeletedNotificationHandler(ILogger<CartDeletedNotificationHandler> logger)
        : INotificationHandler<CartDeletedEvent>
    {
        private readonly ILogger<CartDeletedNotificationHandler> _logger = logger;

        public Task Handle(CartDeletedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _logger.LogCartDeleted(notification.CartId);

            // Add any business logic that should happen when a cart is deleted
            return Task.CompletedTask;
        }
    }
}
