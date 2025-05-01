using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class ComputerProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository
                .FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.ComputersSubCategoryName, cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException(ProductCatalogConstants.ComputersSubCategoryName + " category not found.");

            var laptop = ProductEntity.Create(
                "15.6\" Laptop",
                "15.6-inch laptop with Intel Core i5, 8GB RAM, and 256GB SSD.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}laptop.jpg"),
                category,
                Money.From(499.99m),
                3);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(laptop, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
