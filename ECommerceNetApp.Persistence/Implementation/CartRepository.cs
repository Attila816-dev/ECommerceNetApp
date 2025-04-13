using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;

namespace ECommerceNetApp.Persistence.Implementation
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _dbContext;

        public CartRepository(CartDbContext? dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Cart?> GetByIdAsync(string cartId)
        {
            var collection = _dbContext.GetCollection<Cart>();
            var cart = await collection.FindByIdAsync(cartId).ConfigureAwait(false);
            return cart;
        }

        public async Task SaveAsync(Cart cart)
        {
            ArgumentNullException.ThrowIfNull(cart, nameof(cart));
            ArgumentException.ThrowIfNullOrEmpty(cart.Id, nameof(cart.Id));

            var collection = _dbContext.GetCollection<Cart>();

            // Clear domain events before saving to avoid serialization issues
            var domainEvents = cart.DomainEvents;
            cart.ClearDomainEvents();

            await collection.UpsertAsync(cart).ConfigureAwait(false);

            // Process domain events if needed
            // In a real application, we would publish these events to an event bus
        }

        public async Task DeleteAsync(string cartId)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));

            var collection = _dbContext.GetCollection<Cart>();
            await collection.DeleteAsync(cartId).ConfigureAwait(false);
        }
    }
}
