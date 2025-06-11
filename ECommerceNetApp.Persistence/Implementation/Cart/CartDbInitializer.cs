using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartDbInitializer
    {
        private readonly ICartDbContextFactory _cartDbContextFactory;

        public CartDbInitializer(ICartDbContextFactory cartDbContextFactory)
        {
            _cartDbContextFactory = cartDbContextFactory;
        }

        public async Task InitializeDatabaseAsync()
        {
            // Create Cart collection if it doesn't exist
            using (var dbContext = _cartDbContextFactory.CreateDbContext())
            {
                if (!await dbContext.CollectionExistsAsync<CartEntity>().ConfigureAwait(false))
                {
                    dbContext.CreateCollection<CartEntity>();

                    // You can add indexes here if needed
                    var cartCollection = dbContext.GetCollection<CartEntity>();
                    await cartCollection.EnsureIndexAsync(x => x.Id).ConfigureAwait(false);
                }
            }
        }
    }
}
