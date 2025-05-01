using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class CleaningProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository
                .FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.CleaningSubCategoryName, cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException(ProductCatalogConstants.CleaningSubCategoryName + " category not found.");

            var cleaner = ProductEntity.Create(
                "All-Purpose Cleaner 1L",
                "Powerful all-purpose cleaning solution for all surfaces.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}cleaner.jpg"),
                category,
                Money.From(2.25m),
                50);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(cleaner, cancellationToken).ConfigureAwait(false);

            var dishwasher = ProductEntity.Create(
                "Dishwasher Tablets 40pk",
                "Powerful dishwasher tablets for sparkling clean dishes.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}dishwasher.jpg"),
                category,
                Money.From(6.50m),
                30);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(dishwasher, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
