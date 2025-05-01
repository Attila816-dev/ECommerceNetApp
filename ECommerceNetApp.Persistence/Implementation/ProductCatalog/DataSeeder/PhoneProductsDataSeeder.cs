using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class PhoneProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository.FirstOrDefaultAsync(
                c => c.Name == ProductCatalogConstants.PhonesAndTabletsSubCategoryName,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidOperationException(ProductCatalogConstants.PhonesAndTabletsSubCategoryName + " category not found.");

            var tablet = ProductEntity.Create(
                "10\" Android Tablet",
                "10-inch Android tablet with HD display and 32GB storage.",
                ImageInfo.Create($"{ProductCatalogConstants.PhonesAndTabletsSubCategoryName}tablet.jpg"),
                category,
                Money.From(129.99m),
                5);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(tablet, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
