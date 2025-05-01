using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class DrinkProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository.FirstOrDefaultAsync(
                c => c.Name == ProductCatalogConstants.DrinksSubCategoryName,
                cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException(ProductCatalogConstants.DrinksSubCategoryName + " category not found.");

            var water = ProductEntity.Create(
                "Spring Water 6x1.5L",
                "Natural spring water, refreshing and pure.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}water.jpg"),
                category,
                Money.From(2.75m),
                40);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(water, cancellationToken).ConfigureAwait(false);

            var cola = ProductEntity.Create(
                "Cola 2L",
                "Classic cola soft drink.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}cola.jpg"),
                category,
                Money.From(1.95m),
                35);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(cola, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
