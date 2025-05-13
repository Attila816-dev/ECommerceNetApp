using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class LaundryProductsDataSeeder(ProductCatalogDbContext dbContext)
        : BaseProductsDataSeeder(dbContext), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Household.Laundry, cancellationToken).ConfigureAwait(false);

            var detergent = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Laundry.Detergent,
                "Concentrated laundry detergent for fresh, clean clothes.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}detergent.jpg"),
                category,
                Money.From(5.95m),
                25);

            await AddProductAsync(detergent, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
