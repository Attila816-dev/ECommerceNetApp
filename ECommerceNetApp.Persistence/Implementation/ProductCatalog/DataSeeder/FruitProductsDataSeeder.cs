using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class FruitProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository
                .FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.FruitsAndVegetablesSubCategoryName, cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidOperationException(ProductCatalogConstants.FruitsAndVegetablesSubCategoryName + " category not found.");

            var apples = ProductEntity.Create(
                "Gala Apples 1kg",
                "Sweet and crisp Gala apples, perfect for snacking or cooking.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}apples.jpg"),
                category,
                Money.From(2.50m),
                100);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(apples, cancellationToken).ConfigureAwait(false);

            var bananas = ProductEntity.Create(
                "Bananas 5pk",
                "Ripe and ready-to-eat bananas, perfect for breakfast or as a healthy snack.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}bananas.jpg"),
                category,
                Money.From(1.95m),
                150);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(bananas, cancellationToken).ConfigureAwait(false);

            var oranges = ProductEntity.Create(
                "Navel Oranges 4pk",
                "Sweet and juicy Navel oranges, rich in vitamin C.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}oranges.jpg"),
                category,
                Money.From(3.25m),
                80);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(oranges, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
