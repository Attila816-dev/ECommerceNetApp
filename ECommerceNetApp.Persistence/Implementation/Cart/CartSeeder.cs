using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartSeeder
    {
        private readonly CartDbContext _dbContext;
        private readonly CartDbOptions _cartDbOptions;

        public CartSeeder(CartDbContext dbContext, IOptions<CartDbOptions> cartDbOptions)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _cartDbOptions = cartDbOptions?.Value ?? throw new ArgumentNullException(nameof(cartDbOptions));
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
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
                var sampleCart = CartEntity.Create("sample-cart-1");

                // Add sample items
                var cartImageInfo1 = ImageInfo.Create("https://example.com/product1.jpg", "Sample Product 1 Image");
                sampleCart.AddItem(1, "Sample Product 1", Money.From(19.99m), 1, cartImageInfo1);

                var cartImageInfo2 = ImageInfo.Create("https://example.com/product2.jpg", "Sample Product 2 Image");
                sampleCart.AddItem(2, "Sample Product 2", Money.From(29.99m), 2, cartImageInfo2);

                // Save to database
                await cartCollection.InsertAsync(sampleCart).ConfigureAwait(false);
            }
        }
    }
}
