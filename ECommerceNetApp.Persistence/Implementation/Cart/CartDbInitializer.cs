using CartEntity = ECommerceNetApp.Domain.Entities.Cart;

namespace ECommerceNetApp.Persistence.Implementation.Cart
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
            if (!await _dbContext.CollectionExistsAsync<CartEntity>().ConfigureAwait(false))
            {
                _dbContext.CreateCollection<CartEntity>();

                // You can add indexes here if needed
                var cartCollection = _dbContext.GetCollection<CartEntity>();
                await cartCollection.EnsureIndexAsync(x => x.Id).ConfigureAwait(false);
            }
        }
    }
}
