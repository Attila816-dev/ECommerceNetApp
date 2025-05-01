using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class LaundryProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository.FirstOrDefaultAsync(
                c => c.Name == ProductCatalogConstants.LaundrySubCategoryName,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidOperationException(ProductCatalogConstants.LaundrySubCategoryName + " category not found.");

            var detergent = ProductEntity.Create(
                "Laundry Detergent 2L",
                "Concentrated laundry detergent for fresh, clean clothes.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}detergent.jpg"),
                category,
                Money.From(5.95m),
                25);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(detergent, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
