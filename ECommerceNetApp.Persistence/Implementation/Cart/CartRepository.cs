using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartRepository(
        CartDbContext cartDbContext,
        IDomainEventService domainEventService) : ICartRepository
    {
        private readonly CartDbContext _cartDbContext = cartDbContext ?? throw new ArgumentNullException(nameof(cartDbContext));
        private readonly IDomainEventService _domainEventService = domainEventService ?? throw new ArgumentNullException(nameof(domainEventService));

        public async Task<CartEntity?> GetByIdAsync(string cartId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));

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
            await DispatchDomainEventsAsync(cart, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string cartId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));

            var collection = _cartDbContext.GetCollection<CartEntity>();
            var cart = (await collection.FindByIdAsync(cartId).ConfigureAwait(false))
                ?? throw InvalidCartException.CartNotFound(cartId);

            cart.MarkAsDeleted();
            await collection.DeleteAsync(cartId).ConfigureAwait(false);
            await DispatchDomainEventsAsync(cart, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<bool> ExistsAsync(string cartId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));

            var collection = _cartDbContext.GetCollection<CartEntity>();
            return await collection.ExistsAsync(x => x.Id == cartId).ConfigureAwait(false);
        }

        public async Task<CartItem?> GetCartItemAsync(string cartId, int itemId, CancellationToken cancellationToken)
        {
            var cart = (await GetByIdAsync(cartId, cancellationToken).ConfigureAwait(false)) ?? throw InvalidCartException.CartNotFound(cartId);
            return cart.Items.FirstOrDefault(i => i.Id == itemId);
        }

        private async Task DispatchDomainEventsAsync(CartEntity cart, CancellationToken cancellationToken = default)
        {
            // Publish domain events for tracked entities
            var events = cart.DomainEvents.ToList();
            cart.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _domainEventService.PublishEventAsync(domainEvent, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
