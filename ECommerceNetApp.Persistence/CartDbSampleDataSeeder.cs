using ECommerceNetApp.Domain;

namespace ECommerceNetApp.Persistence
{
    public class CartDbSampleDataSeeder
    {
        private readonly CartDbContext _dbContext;

        public CartDbSampleDataSeeder(CartDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task SeedSampleDataAsync()
        {
            var cartCollection = _dbContext.GetCollection<Cart>();

            // Only seed if collection is empty
            if (cartCollection.Count() == 0)
            {
                // Create a sample cart
                var sampleCart = new Cart 
                { 
                    Id = "sample-cart-1",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
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
                cartCollection.Insert(sampleCart);
            }

            return Task.CompletedTask;
        }
    }
}
