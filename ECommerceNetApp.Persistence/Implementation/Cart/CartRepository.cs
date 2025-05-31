using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartRepository(
        ICartDbContextFactory cartDbContextFactory,
        IEventBus eventBus) : ICartRepository
    {
        private readonly ICartDbContextFactory _cartDbContextFactory = cartDbContextFactory ?? throw new ArgumentNullException(nameof(cartDbContextFactory));
        private readonly IEventBus _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        public async Task<CartEntity?> GetByIdAsync(string cartId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));

            using (var cartDbContext = _cartDbContextFactory.CreateDbContext())
            {
                var collection = cartDbContext.GetCollection<CartEntity>();
                var cart = await collection.FindByIdAsync(cartId).ConfigureAwait(false);
                return cart;
            }
        }

        public async Task SaveAsync(CartEntity cart, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(cart, nameof(cart));
            ArgumentException.ThrowIfNullOrEmpty(cart.Id, nameof(cart.Id));

            using (var cartDbContext = _cartDbContextFactory.CreateDbContext())
            {
                var collection = cartDbContext.GetCollection<CartEntity>();
                await collection.UpsertAsync(cart).ConfigureAwait(false);
            }

            await DispatchDomainEventsAsync(cart, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string cartId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));
            CartEntity cart;

            using (var cartDbContext = _cartDbContextFactory.CreateDbContext())
            {
                var collection = cartDbContext.GetCollection<CartEntity>();
                cart = (await collection.FindByIdAsync(cartId).ConfigureAwait(false))
                    ?? throw InvalidCartException.CartNotFound(cartId);

                cart.MarkAsDeleted();
                await collection.DeleteAsync(cartId).ConfigureAwait(false);
            }

            await DispatchDomainEventsAsync(cart, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<bool> ExistsAsync(string cartId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));

            using (var cartDbContext = _cartDbContextFactory.CreateDbContext())
            {
                var collection = cartDbContext.GetCollection<CartEntity>();
                return await collection.ExistsAsync(x => x.Id == cartId).ConfigureAwait(false);
            }
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            using (var cartDbContext = _cartDbContextFactory.CreateDbContext())
            {
                var collection = cartDbContext.GetCollection<CartEntity>();
                return await collection.CountAsync().ConfigureAwait(false);
            }
        }

        public async Task<CartItem?> GetCartItemAsync(string cartId, int itemId, CancellationToken cancellationToken)
        {
            var cart = (await GetByIdAsync(cartId, cancellationToken).ConfigureAwait(false)) ?? throw InvalidCartException.CartNotFound(cartId);
            return cart.Items.FirstOrDefault(i => i.Id == itemId);
        }

        public async Task<IEnumerable<CartEntity>> GetCartsContainingProductAsync(int productId, CancellationToken cancellationToken)
        {
            using (var cartDbContext = _cartDbContextFactory.CreateDbContext())
            {
                var collection = cartDbContext.GetCollection<CartEntity>();
                var cartItems = await collection.FindAsync(c => c.Items.Select(i => i.Id).Contains(productId)).ConfigureAwait(false);
                return cartItems.ToList();
            }
        }

        private async Task DispatchDomainEventsAsync(CartEntity cart, CancellationToken cancellationToken = default)
        {
            // Publish domain events for tracked entities
            var events = cart.DomainEvents.ToList();
            cart.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _eventBus.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
