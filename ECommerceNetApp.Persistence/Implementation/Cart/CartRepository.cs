using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _dbContext;
        private readonly IDomainEventService _domainEventService;

        public CartRepository(CartDbContext? dbContext, IDomainEventService domainEventService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _domainEventService = domainEventService ?? throw new ArgumentNullException(nameof(domainEventService));
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

            await collection.UpsertAsync(cart).ConfigureAwait(false);
            await _domainEventService.PublishEventsAsync(cart, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string cartId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(cartId, nameof(cartId));

            var collection = _dbContext.GetCollection<CartEntity>();
            var cart = await collection.FindByIdAsync(cartId).ConfigureAwait(false);
            if (cart == null)
            {
                throw new CartNotFoundException();
            }

            cart.MarkAsDeleted();

            await collection.DeleteAsync(cartId).ConfigureAwait(false);
            await _domainEventService.PublishEventsAsync(cart, cancellationToken).ConfigureAwait(false);
        }
    }
}
