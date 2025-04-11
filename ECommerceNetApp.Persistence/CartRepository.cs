using ECommerceNetApp.Domain;

namespace ECommerceNetApp.Persistence
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _dbContext;

        public CartRepository(CartDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task<Cart> GetCartAsync(string cartId)
        {
            var collection = _dbContext.GetCollection<Cart>();
            var cart = collection.FindById(cartId);
            return Task.FromResult(cart);
        }

        public Task SaveCartAsync(Cart cart)
        {
            if (string.IsNullOrEmpty(cart.Id))
            {
                throw new ArgumentException("Cart Id cannot be empty", nameof(cart));
            }

            cart.UpdatedAt = DateTime.UtcNow;
            var collection = _dbContext.GetCollection<Cart>();

            collection.Upsert(cart);

            return Task.CompletedTask;
        }

        public Task DeleteCartAsync(string cartId)
        {
            var collection = _dbContext.GetCollection<Cart>();
            collection.Delete(cartId);
            return Task.CompletedTask;
        }
    }
}

