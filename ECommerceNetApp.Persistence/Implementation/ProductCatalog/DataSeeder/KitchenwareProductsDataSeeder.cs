using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class KitchenwareProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository.FirstOrDefaultAsync(
                c => c.Name == ProductCatalogConstants.KitchenwareSubCategoryName,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidOperationException(ProductCatalogConstants.KitchenwareSubCategoryName + " category not found.");

            var pan = ProductEntity.Create(
                "Non-Stick Frying Pan 28cm",
                "Durable non-stick frying pan, suitable for all hob types.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}pan.jpg"),
                category,
                Money.From(15.99m),
                10);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(pan, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
