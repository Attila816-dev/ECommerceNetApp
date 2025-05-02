using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class DairyProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : BaseProductsDataSeeder(productCatalogUnitOfWork), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Groceries.Dairy, cancellationToken).ConfigureAwait(false);

            var milk = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Dairy.Milk,
                "Fresh semi-skimmed milk, locally sourced.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}milk.jpg"),
                category,
                Money.From(1.85m),
                50);

            await AddProductAsync(milk, cancellationToken).ConfigureAwait(false);

            var eggs = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Dairy.Egg,
                "Large free-range eggs from happy hens.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}eggs.jpg"),
                category,
                Money.From(2.75m),
                40);

            await AddProductAsync(eggs, cancellationToken).ConfigureAwait(false);

            var cheese = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Dairy.Cheese,
                "Tangy mature cheddar cheese, perfect for sandwiches or cooking.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}cheese.jpg"),
                category,
                Money.From(3.50m),
                60);

            await AddProductAsync(cheese, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
