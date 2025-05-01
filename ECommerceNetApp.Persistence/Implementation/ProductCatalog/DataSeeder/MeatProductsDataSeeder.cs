using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class MeatProductsDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await productCatalogUnitOfWork.CategoryRepository.FirstOrDefaultAsync(
                c => c.Name == ProductCatalogConstants.MeatSubCategoryName,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidOperationException(ProductCatalogConstants.MeatSubCategoryName + " category not found.");

            var chicken = ProductEntity.Create(
                "Chicken Breast Fillets 500g",
                "Fresh chicken breast fillets, perfect for a variety of dishes.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}chicken.jpg"),
                category,
                Money.From(5.95m),
                20);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(chicken, cancellationToken).ConfigureAwait(false);

            var beef = ProductEntity.Create(
                "Lean Beef Mince 500g",
                "Quality lean beef mince, ideal for bolognese or burgers.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}beef.jpg"),
                category,
                Money.From(4.50m),
                15);

            await productCatalogUnitOfWork.ProductRepository.AddAsync(beef, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
