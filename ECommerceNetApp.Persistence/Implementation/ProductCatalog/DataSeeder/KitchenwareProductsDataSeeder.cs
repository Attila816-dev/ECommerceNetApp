using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class KitchenwareProductsDataSeeder(ProductCatalogDbContext dbContext)
        : BaseProductsDataSeeder(dbContext), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Household.Kitchenware, cancellationToken).ConfigureAwait(false);

            var pan = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Kitchenware.FryingPan,
                "Durable non-stick frying pan, suitable for all hob types.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}pan.jpg"),
                category,
                Money.From(15.99m),
                10);

            await AddProductAsync(pan, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
