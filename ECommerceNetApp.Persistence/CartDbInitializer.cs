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

        public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
        {
            // Create Cart collection if it doesn't exist
            if (!await _dbContext.CollectionExistsAsync<Cart>().ConfigureAwait(false))
            {
                _dbContext.CreateCollection<Cart>();

                // You can add indexes here if needed
                var cartCollection = _dbContext.GetCollection<Cart>();
                await cartCollection.EnsureIndexAsync(x => x.Id).ConfigureAwait(false);
            }
        }
    }
}
