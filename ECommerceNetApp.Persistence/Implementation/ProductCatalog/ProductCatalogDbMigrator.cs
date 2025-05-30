using ECommerceNetApp.Domain.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class ProductCatalogDbMigrator(
        ProductCatalogDbContext dbContext,
        IOptions<ProductCatalogDbOptions> options)
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;
        private readonly IOptions<ProductCatalogDbOptions> _options = options;

        public async Task MigrateDatabaseAsync(CancellationToken cancellationToken)
        {
            if (_options.Value.EnableDatabaseMigration)
            {
                await _dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
