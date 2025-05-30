using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class CleaningProductsDataSeeder(ProductCatalogDbContext dbContext)
        : BaseProductsDataSeeder(dbContext), IProductDataSeeder
    {
        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Household.Cleaning, cancellationToken).ConfigureAwait(false);

            var cleaner = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Cleaning.Cleaner,
                "Powerful all-purpose cleaning solution for all surfaces.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}cleaner.jpg"),
                category,
                Money.From(2.25m),
                50);

            await AddProductAsync(cleaner, cancellationToken).ConfigureAwait(false);

            var dishwasher = ProductEntity.Create(
                ProductCatalogConstants.ProductNames.Cleaning.DishwasherTablets,
                "Powerful dishwasher tablets for sparkling clean dishes.",
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Product}dishwasher.jpg"),
                category,
                Money.From(6.50m),
                30);

            await AddProductAsync(dishwasher, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
