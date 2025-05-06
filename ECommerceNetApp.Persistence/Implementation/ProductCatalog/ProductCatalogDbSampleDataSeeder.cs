using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class ProductCatalogDbSampleDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext;
        private readonly ProductCatalogDbOptions _productCatalogDbOptions;

        public ProductCatalogDbSampleDataSeeder(ProductCatalogDbContext dbContext, IOptions<ProductCatalogDbOptions> productCatalogDbOptions)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _productCatalogDbOptions = productCatalogDbOptions?.Value ?? throw new ArgumentNullException(nameof(productCatalogDbOptions));
        }

        public async Task SeedSampleDataAsync(CancellationToken cancellationToken = default)
        {
            if (!_productCatalogDbOptions.SeedSampleData)
            {
                return; // Skip seeding if disabled
            }

            if (!await _dbContext.Categories.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                await _dbContext.Categories.AddRangeAsync(
                    CategoryEntity.Create("Electronics", null, null),
                    CategoryEntity.Create("Books", null, null),
                    CategoryEntity.Create("Clothing", null, null))
                    .ConfigureAwait(false);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
