using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class BakeryProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository
                .FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.BakerySubCategoryName, cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException(ProductCatalogConstants.BakerySubCategoryName + " category not found.");

            var bread = ProductEntity.Create(
                "Wholemeal Bread 800g",
                "Freshly baked wholemeal bread, ideal for sandwiches.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}bread.jpg"),
                category,
                Money.From(1.20m),
                30);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(bread, cancellationToken).ConfigureAwait(false);

            var croissants = ProductEntity.Create(
                "Butter Croissants 4pk",
                "Flaky butter croissants, baked in-store daily.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}croissants.jpg"),
                category,
                Money.From(2.50m),
                25);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(croissants, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
