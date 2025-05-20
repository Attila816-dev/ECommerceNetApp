using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog.EntityTypeConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class ProductCatalogDbContext : DbContext
    {
        private static readonly Action<ILogger, Exception?> LogEbentPublishingError =
            LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(0, nameof(ProductCatalogDbContext)),
            "Error during publishing entity event");

        private readonly IEventBus? _eventBus;
        private readonly ILogger<ProductCatalogDbContext>? _logger;

        public ProductCatalogDbContext(
            DbContextOptions<ProductCatalogDbContext> options,
            IEventBus? eventBus,
            ILogger<ProductCatalogDbContext>? logger)
            : base(options)
        {
            _eventBus = eventBus;
            _logger = logger;
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
            if (_eventBus == null)
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
#pragma warning disable CA1031 // Do not catch general exception types
                    try
                    {
                        await _eventBus.PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogEbentPublishingError(_logger!, ex);
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }
            }
        }
    }
}
