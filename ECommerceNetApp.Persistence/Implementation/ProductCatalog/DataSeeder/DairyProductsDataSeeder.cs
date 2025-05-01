using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class DairyProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository
                .FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.DairySubCategoryName, cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidOperationException(ProductCatalogConstants.DairySubCategoryName + " category not found.");

            var milk = ProductEntity.Create(
                "Semi-Skimmed Milk 2L",
                "Fresh semi-skimmed milk, locally sourced.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}milk.jpg"),
                category,
                Money.From(1.85m),
                50);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(milk, cancellationToken).ConfigureAwait(false);

            var eggs = ProductEntity.Create(
                "Free Range Eggs 12pk",
                "Large free-range eggs from happy hens.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}eggs.jpg"),
                category,
                Money.From(2.75m),
                40);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(eggs, cancellationToken).ConfigureAwait(false);

            var cheese = ProductEntity.Create(
                "Mature Cheddar 400g",
                "Tangy mature cheddar cheese, perfect for sandwiches or cooking.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}cheese.jpg"),
                category,
                Money.From(3.50m),
                60);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(cheese, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
