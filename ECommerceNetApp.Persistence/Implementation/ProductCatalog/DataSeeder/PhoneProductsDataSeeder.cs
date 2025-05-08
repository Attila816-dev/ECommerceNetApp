using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class PhoneProductsDataSeeder(ProductCatalogDbContext dbContext)
        : BaseProductsDataSeeder(dbContext), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Electronics.PhonesAndTablets, cancellationToken).ConfigureAwait(false);

            var tablet = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.PhonesAndTablets.AndroidTablet,
                "10-inch Android tablet with HD display and 32GB storage.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}tablet.jpg"),
                category,
                Money.From(129.99m),
                5);

            await AddProductAsync(tablet, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
