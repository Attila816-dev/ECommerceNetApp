using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Persistence.Implementation.Cart
{
    public class CartUnitOfWork : ICartUnitOfWork
    {
        private readonly CartDbContext _cartDbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<CartEntity> _trackedEntities = new(); // Track modified entities
        private bool _disposedValue;

        public CartUnitOfWork(CartDbContext cartDbContext, IServiceProvider serviceProvider)
        {
            _cartDbContext = cartDbContext ?? throw new ArgumentNullException(nameof(cartDbContext));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            CartRepository = new CartRepository(_cartDbContext, this);
        }

        public ICartRepository CartRepository { get; }

        public void TrackEntity(CartEntity cartEntity)
        {
            if (!_trackedEntities.Contains(cartEntity))
            {
                _trackedEntities.Add(cartEntity);
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var domainEventService = scope.ServiceProvider.GetRequiredService<IDomainEventService>();

            // Publish domain events for tracked entities
            foreach (var cart in _trackedEntities)
            {
                if (cart.DomainEvents.Count > 0)
                {
                    var domainEvents = cart.DomainEvents.ToList();
                    cart.ClearDomainEvents();

                    foreach (var domainEvent in domainEvents)
                    {
                        await domainEventService.PublishEventAsync(domainEvent, cancellationToken).ConfigureAwait(false);
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
