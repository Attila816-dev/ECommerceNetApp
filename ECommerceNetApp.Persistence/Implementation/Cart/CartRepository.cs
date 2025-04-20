using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Persistence.Interfaces.Cart;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartRepository(CartDbContext cartDbContext, ICartUnitOfWork cartUnitOfWork) : ICartRepository
    {
        private readonly CartDbContext _cartDbContext = cartDbContext;
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

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

            // Track the modified entity
            _cartUnitOfWork.TrackEntity(cart);
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

            // Track the modified entity
            _cartUnitOfWork.TrackEntity(cart);
        }
    }
}
