using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class DrinkProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : BaseProductsDataSeeder(productCatalogUnitOfWork), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Groceries.Drinks, cancellationToken).ConfigureAwait(false);

            var water = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Drink.Water,
                "Natural spring water, refreshing and pure.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}water.jpg"),
                category,
                Money.From(2.75m),
                40);

            await AddProductAsync(water, cancellationToken).ConfigureAwait(false);

            var cola = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Drink.Cola,
                "Classic cola soft drink.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}cola.jpg"),
                category,
                Money.From(1.95m),
                35);

            await AddProductAsync(cola, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
