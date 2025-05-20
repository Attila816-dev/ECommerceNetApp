using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class MeatProductsDataSeeder(ProductCatalogDbContext dbContext)
        : BaseProductsDataSeeder(dbContext), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Groceries.Meat, cancellationToken).ConfigureAwait(false);

            var chicken = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Meat.ChickenBreast,
                "Fresh chicken breast fillets, perfect for a variety of dishes.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}chicken.jpg"),
                category,
                Money.From(5.95m),
                20);

            await AddProductAsync(chicken, cancellationToken).ConfigureAwait(false);

            var beef = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Meat.BeefMince,
                "Quality lean beef mince, ideal for bolognese or burgers.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}beef.jpg"),
                category,
                Money.From(4.50m),
                15);

            await AddProductAsync(beef, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
