using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class ProductCatalogUnitOfWork : IProductCatalogUnitOfWork, IDisposable
    {
        private readonly ProductCatalogDbContext _dbContext;
        private readonly IDomainEventService _domainEventService;
        private bool disposedValue;

        public ProductCatalogUnitOfWork(ProductCatalogDbContext dbContext, IDomainEventService domainEventService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _domainEventService = domainEventService ?? throw new ArgumentNullException(nameof(domainEventService));

            ProductRepository = new ProductRepository(_dbContext);
            CategoryRepository = new CategoryRepository(_dbContext);
        }

        public IProductRepository ProductRepository { get; }

        public ICategoryRepository CategoryRepository { get; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            // Save changes to the database
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Publish domain events after successful save
            var entitiesWithEvents = _dbContext.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Count > 0)
                .Select(e => e.Entity)
                .ToList();

            foreach (var entity in entitiesWithEvents)
            {
                var domainEvents = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                foreach (var domainEvent in domainEvents)
                {
                    await _domainEventService.PublishEventAsync(domainEvent, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _dbContext.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }
    }
}
