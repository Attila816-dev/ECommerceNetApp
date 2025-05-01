using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class ProductCatalogDbMigrator
    {
        private readonly ProductCatalogDbContext _dbContext;

        public ProductCatalogDbMigrator(ProductCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task MigrateDatabaseAsync(CancellationToken cancellationToken)
        {
            await _dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
