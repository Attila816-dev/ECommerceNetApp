using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;

namespace ECommerceNetApp.Persistence.Implementation
{
    public class CartDbSampleDataSeeder
    {
        private readonly CartDbContext _dbContext;

        public CartDbSampleDataSeeder(CartDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedSampleDataAsync()
        {
            var cartCollection = _dbContext.GetCollection<Cart>();

            // Only seed if collection is empty
            if (await cartCollection.CountAsync().ConfigureAwait(false) == 0)
            {
                // Create a sample cart
                var sampleCart = new Cart("sample-cart-1");

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
