using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class HouseholdCategoriesDataSeeder(ProductCatalogDbContext dbContext)
        : BaseCategoriesDataSeeder(dbContext), ICategoryDataSeeder
    {
        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Root.HouseholdCategoryName, cancellationToken).ConfigureAwait(false);

            // Household subcategories
            var cleaning = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Household.Cleaning,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}cleaning.jpg"),
                parentCategory);

            await AddCategoryAsync(cleaning, cancellationToken).ConfigureAwait(false);

            var laundry = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Household.Laundry,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}laundry.jpg"),
                parentCategory);

            await AddCategoryAsync(laundry, cancellationToken).ConfigureAwait(false);

            var kitchenware = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Household.Kitchenware,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}kitchenware.jpg"),
                parentCategory);

            await AddCategoryAsync(kitchenware, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
