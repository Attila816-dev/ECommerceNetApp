using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartDbSampleDataSeeder
    {
        private readonly CartDbContext _dbContext;
        private readonly CartDbOptions _cartDbOptions;

        public CartDbSampleDataSeeder(CartDbContext dbContext, IOptions<CartDbOptions> cartDbOptions)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _cartDbOptions = cartDbOptions?.Value ?? throw new ArgumentNullException(nameof(cartDbOptions));
        }

        public async Task SeedSampleDataAsync(CancellationToken cancellationToken = default)
        {
            if (!_cartDbOptions.SeedSampleData)
            {
                return; // Skip seeding if disabled
            }

            var cartCollection = _dbContext.GetCollection<CartEntity>();

            // Only seed if collection is empty
            if (await cartCollection.CountAsync().ConfigureAwait(false) == 0)
            {
                // Create a sample cart
                var sampleCart = new CartEntity("sample-cart-1");

                // Add sample items
                var cartImageInfo1 = new ImageInfo("https://example.com/product1.jpg", "Sample Product 1 Image");
                sampleCart.AddItem(new CartItem(1, "Sample Product 1", new Money(19.99m), 1, cartImageInfo1));

                var cartImageInfo2 = new ImageInfo("https://example.com/product2.jpg", "Sample Product 2 Image");
                sampleCart.AddItem(new CartItem(2, "Sample Product 2", new Money(29.99m), 2, cartImageInfo2));

                // Save to database
                await cartCollection.InsertAsync(sampleCart).ConfigureAwait(false);
            }
        }
    }
}
