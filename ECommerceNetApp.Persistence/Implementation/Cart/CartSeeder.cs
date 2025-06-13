using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartSeeder
    {
        private static readonly Action<ILogger, string, Exception?> LogCartCreated =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, nameof(LogCartCreated)), "Created cart with ID: {CartId}");

        private readonly ICartRepository _cartRepository;
        private readonly ProductCatalogDbContext _productCatalogDbContext;
        private readonly ILogger<CartSeeder> _logger;
        private readonly CartDbOptions _cartDbOptions;

        public CartSeeder(
            ICartRepository cartRepository,
            ProductCatalogDbContext productCatalogDbContext,
            IOptions<CartDbOptions> cartDbOptions,
            ILogger<CartSeeder> logger)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _productCatalogDbContext = productCatalogDbContext ?? throw new ArgumentNullException(nameof(productCatalogDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cartDbOptions = cartDbOptions?.Value ?? throw new ArgumentNullException(nameof(cartDbOptions));
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            if (!_cartDbOptions.SeedSampleData)
            {
                return; // Skip seeding if disabled
            }

            // Only seed if collection is empty
            if (await _cartRepository.CountAsync(cancellationToken).ConfigureAwait(false) == 0)
            {
                // Demo cart 1 - Guest user
                string guestCartId = "guest-cart-12345";
                bool guestCartExists = await _cartRepository.ExistsAsync(guestCartId, cancellationToken).ConfigureAwait(false);

                if (!guestCartExists)
                {
                    var guestCart = CartEntity.Create(guestCartId);

                    // Add some sample items to the guest cart
                    await AddProductToCartAsync(
                        guestCart,
                        ProductCatalogConstants.ProductNames.FruitesAndVegetables.Apple,
                        2,
                        cancellationToken)
                        .ConfigureAwait(false);

                    await AddProductToCartAsync(
                        guestCart,
                        ProductCatalogConstants.ProductNames.Dairy.Milk,
                        1,
                        cancellationToken)
                        .ConfigureAwait(false);

                    await _cartRepository.SaveAsync(guestCart, CancellationToken.None).ConfigureAwait(false);
                    LogCartCreated.Invoke(_logger, guestCartId, null);
                }

                // Demo cart 2 - Registered user
                string userCartId = "user-cart-67890";
                bool userCartExists = await _cartRepository.ExistsAsync(userCartId, CancellationToken.None).ConfigureAwait(false);

                if (!userCartExists)
                {
                    var userCart = CartEntity.Create(userCartId);

                    // Add some sample items to the user cart
                    await AddProductToCartAsync(
                        userCart,
                        ProductCatalogConstants.ProductNames.Dairy.Egg,
                        1,
                        cancellationToken)
                        .ConfigureAwait(false);

                    await AddProductToCartAsync(
                        userCart,
                        ProductCatalogConstants.ProductNames.Meat.ChickenBreast,
                        2,
                        cancellationToken)
                        .ConfigureAwait(false);

                    await AddProductToCartAsync(
                        userCart,
                        ProductCatalogConstants.ProductNames.Bakery.Bread,
                        1,
                        cancellationToken)
                        .ConfigureAwait(false);

                    await _cartRepository.SaveAsync(userCart, CancellationToken.None).ConfigureAwait(false);
                    LogCartCreated.Invoke(_logger, userCartId, null);
                }
            }
        }

        private async Task AddProductToCartAsync(CartEntity cart, string productName, int quantity, CancellationToken cancellationToken)
        {
            var product = (await _productCatalogDbContext.Products
                .FirstOrDefaultAsync(p => p.Name == productName, cancellationToken: cancellationToken)
                .ConfigureAwait(false)) ?? throw new InvalidOperationException(productName + " product not found.");

            cart.AddItem(product.Id, product.Name, product.Price, quantity, product.Image);
        }
    }
}