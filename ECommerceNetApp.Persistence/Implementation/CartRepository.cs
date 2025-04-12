using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;

namespace ECommerceNetApp.Persistence.Implementation
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _cartDbContext;

        public CartRepository(CartDbContext? cartDbContext)
        {
            _cartDbContext = cartDbContext ?? throw new ArgumentNullException(nameof(cartDbContext));
        }

        public async Task<Cart?> GetCartAsync(string cartId)
        {
            var collection = _cartDbContext.GetCollection<Cart>();
            var cart = await collection.FindByIdAsync(cartId).ConfigureAwait(false);
            return cart;
        }

        public async Task SaveCartAsync(Cart cart)
        {
            if (string.IsNullOrEmpty(cart?.Id))
            {
                throw new ArgumentException("Cart Id cannot be empty", nameof(cart));
            }

            var collection = _cartDbContext.GetCollection<Cart>();

            await collection.UpsertAsync(cart).ConfigureAwait(false);
        }

        public async Task DeleteCartAsync(string cartId)
        {
            var collection = _cartDbContext.GetCollection<Cart>();
            await collection.DeleteAsync(cartId).ConfigureAwait(false);
        }
    }
}
