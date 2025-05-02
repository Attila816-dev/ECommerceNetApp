using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class FruitProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : BaseProductsDataSeeder(productCatalogUnitOfWork), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Groceries.FruitsAndVegetables, cancellationToken).ConfigureAwait(false);

            var apples = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.FruitesAndVegetables.Apple,
                "Sweet and crisp Gala apples, perfect for snacking or cooking.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}apples.jpg"),
                category,
                Money.From(2.50m),
                100);

            await AddProductAsync(apples, cancellationToken).ConfigureAwait(false);

            var bananas = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.FruitesAndVegetables.Banana,
                "Ripe and ready-to-eat bananas, perfect for breakfast or as a healthy snack.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}bananas.jpg"),
                category,
                Money.From(1.95m),
                150);

            await AddProductAsync(bananas, cancellationToken).ConfigureAwait(false);

            var oranges = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.FruitesAndVegetables.Orange,
                "Sweet and juicy Navel oranges, rich in vitamin C.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}oranges.jpg"),
                category,
                Money.From(3.25m),
                80);

            await AddProductAsync(oranges, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
