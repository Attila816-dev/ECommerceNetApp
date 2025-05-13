using System.Text.Json;
using ECommerceNetApp.Domain.Events.Product;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.NotificationHandlers.Product
{
    public class ProductUpdatedNotificationHandler(ILogger<ProductUpdatedNotificationHandler> logger, ICartRepository cartRepository)
        : INotificationHandler<ProductUpdatedEvent>
    {
        // Define a static LoggerMessage delegate for improved performance and consistent templates
        private static readonly Action<ILogger, int, string, Exception?> LogProductCreated =
            LoggerMessage.Define<int, string>(
                LogLevel.Information,
                new EventId(1, nameof(ProductUpdatedNotificationHandler)),
                "Product {ProductId} updated with properties: {SerializedNotification}");

        private static readonly Action<ILogger, int, Exception?> LogUpdatingCarts =
           LoggerMessage.Define<int>(
               LogLevel.Debug,
               new EventId(2, nameof(ProductUpdatedNotificationHandler)),
               "Updating product {ProductId} in all carts");

        private static readonly Action<ILogger, int, string, Exception?> LogUpdatedCart =
           LoggerMessage.Define<int, string>(
               LogLevel.Information,
               new EventId(3, nameof(ProductUpdatedNotificationHandler)),
               "Updated product {ProductId} in cart {CartId}");

        private static readonly Action<ILogger, int, string, Exception?> LogErrorDuringCartUpdate =
           LoggerMessage.Define<int, string>(
               LogLevel.Error,
               new EventId(4, nameof(ProductUpdatedNotificationHandler)),
               "Error occurred during updating product {ProductId} in cart {CartId}");

        private static readonly Action<ILogger, int, Exception?> LogProductUpdatedEventError =
           LoggerMessage.Define<int>(
               LogLevel.Error,
               new EventId(5, nameof(ProductUpdatedNotificationHandler)),
               "Error occurred during handling ProductUpdatedEvent for product {ProductId}");

        private readonly ILogger<ProductUpdatedNotificationHandler> _logger = logger;
        private readonly ICartRepository _cartRepository = cartRepository;

        public void Register(IEventBus eventBus)
        {
            ArgumentNullException.ThrowIfNull(eventBus, nameof(eventBus));
            eventBus.Register(this);
        }

        public async Task HandleAsync(ProductUpdatedEvent notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                LogProductCreated(
                    _logger,
                    notification.ProductId,
                    JsonSerializer.Serialize(notification),
                    null);

                LogUpdatingCarts(_logger, notification.ProductId, null);

                // Get all carts containing this product
                var cartsWithProduct = await _cartRepository.GetCartsContainingProductAsync(notification.ProductId, cancellationToken).ConfigureAwait(false);

                foreach (var cart in cartsWithProduct)
                {
                    try
                    {
                        var cartItem = cart.Items.FirstOrDefault(i => i.Id == notification.ProductId);
                        if (cartItem != null)
                        {
                            cart.UpdateItemProperties(notification.ProductId, notification.Name, notification.Price, notification.ImageInfo);
                            await _cartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
                            LogUpdatedCart(_logger, notification.ProductId, cart.Id, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogErrorDuringCartUpdate(_logger, notification.ProductId, cart.Id, ex);
                        throw; // Re-throw to trigger service bus retry policy
                    }
                }
            }
            catch (Exception ex)
            {
                LogProductUpdatedEventError(_logger, notification.ProductId, ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
