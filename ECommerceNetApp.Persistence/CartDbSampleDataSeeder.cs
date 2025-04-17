using ECommerceNetApp.Domain;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Persistence
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

            var cartCollection = _dbContext.GetCollection<Cart>();

            // Only seed if collection is empty
            if ((await cartCollection.CountAsync().ConfigureAwait(false)) == 0)
            {
                // Create a sample cart
                var sampleCart = new Cart
                {
                    Id = "sample-cart-1",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                // Add sample items
                sampleCart.Items.Add(new CartItem
                {
                    Id = 1,
                    Name = "Sample Product 1",
                    Price = 19.99m,
                    Quantity = 1,
                    ImageAltText = "Sample Product 1 Image",
                    ImageUrl = "https://example.com/product1.jpg",
                });

                sampleCart.Items.Add(new CartItem
                {
                    Id = 2,
                    Name = "Sample Product 2",
                    Price = 29.99m,
                    Quantity = 2,
                    ImageAltText = "Sample Product 2 Image",
                    ImageUrl = "https://example.com/product2.jpg",
                });

                // Save to database
                await cartCollection.InsertAsync(sampleCart).ConfigureAwait(false);
            }
        }
    }
}
