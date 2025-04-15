using ECommerceNetApp.Persistence.Interfaces;
using CartEntity = ECommerceNetApp.Domain.Entities.Cart;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _dbContext;

        public CartRepository(CartDbContext? dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<CartEntity?> GetByIdAsync(string cartId, CancellationToken cancellationToken)
        {
            var collection = _dbContext.GetCollection<CartEntity>();
            var cart = await collection.FindByIdAsync(cartId).ConfigureAwait(false);
            return cart;
        }

        public async Task SaveAsync(CartEntity cart, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(cart, nameof(cart));
            ArgumentException.ThrowIfNullOrEmpty(cart.Id, nameof(cart.Id));

            var collection = _dbContext.GetCollection<CartEntity>();

            // Clear domain events before saving to avoid serialization issues
            var domainEvents = cart.DomainEvents;
            cart.ClearDomainEvents();

            await collection.UpsertAsync(cart).ConfigureAwait(false);

            // Process domain events if needed
            // In a real application, we would publish these events to an event bus
        }

        public async Task DeleteAsync(string cartId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));

            var collection = _dbContext.GetCollection<CartEntity>();
            await collection.DeleteAsync(cartId).ConfigureAwait(false);
        }
    }
}
