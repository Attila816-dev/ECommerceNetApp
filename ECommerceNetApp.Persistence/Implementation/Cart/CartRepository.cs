using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Persistence.Interfaces.Cart;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartRepository(CartDbContext cartDbContext) : ICartRepository
    {
        private readonly CartDbContext _cartDbContext = cartDbContext;

        public async Task<CartEntity?> GetByIdAsync(string cartId, CancellationToken cancellationToken)
        {
            var collection = _cartDbContext.GetCollection<CartEntity>();
            var cart = await collection.FindByIdAsync(cartId).ConfigureAwait(false);
            return cart;
        }

        public async Task SaveAsync(CartEntity cart, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(cart, nameof(cart));
            ArgumentException.ThrowIfNullOrEmpty(cart.Id, nameof(cart.Id));

            var collection = _cartDbContext.GetCollection<CartEntity>();

            await collection.UpsertAsync(cart).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string cartId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));

            var collection = _cartDbContext.GetCollection<CartEntity>();
            var cart = await collection.FindByIdAsync(cartId).ConfigureAwait(false);
            if (cart == null)
            {
                throw new CartNotFoundException();
            }

            cart.MarkAsDeleted();

            await collection.DeleteAsync(cartId).ConfigureAwait(false);
        }
    }
}
