using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class ComputerProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : BaseProductsDataSeeder(productCatalogUnitOfWork), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Electronics.Computers, cancellationToken).ConfigureAwait(false);

            var laptop = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Computer.Laptop,
                "15.6-inch laptop with Intel Core i5, 8GB RAM, and 256GB SSD.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}laptop.jpg"),
                category,
                Money.From(499.99m),
                3);

            await AddProductAsync(laptop, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
