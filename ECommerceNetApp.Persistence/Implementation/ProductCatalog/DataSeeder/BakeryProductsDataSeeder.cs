using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class BakeryProductsDataSeeder(ProductCatalogDbContext dbContext)
        : BaseProductsDataSeeder(dbContext), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Groceries.Bakery, cancellationToken).ConfigureAwait(false);

            var bread = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Bakery.Bread,
                "Freshly baked wholemeal bread, ideal for sandwiches.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}bread.jpg"),
                category,
                Money.From(1.20m),
                30);

            await AddProductAsync(bread, cancellationToken).ConfigureAwait(false);

            var croissants = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Bakery.Croissant,
                "Flaky butter croissants, baked in-store daily.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}croissants.jpg"),
                category,
                Money.From(2.50m),
                25);

            await AddProductAsync(croissants, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
