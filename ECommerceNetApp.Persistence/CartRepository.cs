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

        public async Task<Cart> GetCartAsync(string cartId)
        {
            var collection = _dbContext.GetCollection<Cart>();
            var cart = await collection.FindByIdAsync(cartId);
            return cart;
        }

        public async Task SaveCartAsync(Cart cart)
        {
            if (string.IsNullOrEmpty(cart.Id))
            {
                throw new ArgumentException("Cart Id cannot be empty", nameof(cart));
            }

            cart.UpdatedAt = DateTime.UtcNow;
            var collection = _dbContext.GetCollection<Cart>();

            await collection.UpsertAsync(cart);
        }

        public async Task DeleteCartAsync(string cartId)
        {
            var collection = _dbContext.GetCollection<Cart>();
            await collection.DeleteAsync(cartId);
        }
    }
}

