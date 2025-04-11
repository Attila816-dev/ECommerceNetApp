using ECommerceNetApp.Domain;

namespace ECommerceNetApp.Persistence
{
    public class CartDbInitializer
    {
        private readonly CartDbContext _dbContext;

        public CartDbInitializer(CartDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task InitializeDatabaseAsync()
        {
            // Create Cart collection if it doesn't exist
            if (!_dbContext.CollectionExists("Cart"))
            {
                _dbContext.CreateCollection("Cart");

                // You can add indexes here if needed
                var cartCollection = _dbContext.GetCollection<Cart>();
                cartCollection.EnsureIndex(x => x.Id);
            }

            return Task.CompletedTask;
        }
    }
}
