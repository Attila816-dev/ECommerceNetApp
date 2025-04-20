using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartUnitOfWork(
        CartDbContext cartDbContext,
        IDomainEventService domainEventService,
        ICartRepositoryFactory cartRepositoryFactory)
        : ICartUnitOfWork
    {
        private readonly CartDbContext _cartDbContext = cartDbContext;
        private readonly IDomainEventService _domainEventService = domainEventService;
        private readonly ICartRepositoryFactory _cartRepositoryFactory = cartRepositoryFactory;
        private readonly List<CartEntity> _trackedEntities = new(); // Track modified entities
        private bool _disposedValue;
        private ICartRepository? _cartRepository;

        public ICartRepository CartRepository =>
            _cartRepository ??= _cartRepositoryFactory.CreateRepository(this);

        public void TrackEntity(CartEntity cartEntity)
        {
            if (!_trackedEntities.Contains(cartEntity))
            {
                _trackedEntities.Add(cartEntity);
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            // Publish domain events for tracked entities
            foreach (var cart in _trackedEntities)
            {
                if (cart.DomainEvents.Count > 0)
                {
                    var domainEvents = cart.DomainEvents.ToList();
                    cart.ClearDomainEvents();

                    foreach (var domainEvent in domainEvents)
                    {
                        await _domainEventService.PublishEventAsync(domainEvent, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            // Clear the tracked entities after publishing events
            _trackedEntities.Clear();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cartDbContext.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
