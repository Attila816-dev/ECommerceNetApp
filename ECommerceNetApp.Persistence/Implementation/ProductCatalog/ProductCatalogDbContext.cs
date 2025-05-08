using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog.EntityTypeConfiguration;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class ProductCatalogDbContext : DbContext
    {
        private readonly IDomainEventService? _domainEventService;

        public ProductCatalogDbContext(
            DbContextOptions<ProductCatalogDbContext> options,
            IDomainEventService? domainEventService)
            : base(options)
        {
            _domainEventService = domainEventService;
        }

        public virtual DbSet<CategoryEntity> Categories { get; set; } = null!;

        public virtual DbSet<ProductEntity> Products { get; set; } = null!;

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
            await DispatchDomainEventsAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CategoryEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProductEntityTypeConfiguration());
        }

        private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        {
            if (_domainEventService == null)
            {
                return;
            }

            var entitiesWithEvents = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity.DomainEvents.Count > 0)
                .Select(e => e.Entity)
                .ToList();

            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                foreach (var domainEvent in events)
                {
                    await _domainEventService.PublishEventAsync(domainEvent, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
