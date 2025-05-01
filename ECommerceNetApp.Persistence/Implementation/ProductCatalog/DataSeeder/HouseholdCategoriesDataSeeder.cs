using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class HouseholdCategoriesDataSeeder(ProductCatalogDbContext dbContext) : ICategoryDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.HouseholdCategoryName, cancellationToken).ConfigureAwait(false);
            if (parentCategory == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.HouseholdCategoryName + " category not found.");
            }

            // Household subcategories
            var cleaning = CategoryEntity.Create(
                ProductCatalogConstants.CleaningSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}cleaning.jpg"),
                parentCategory);

            var laundry = CategoryEntity.Create(
                ProductCatalogConstants.LaundrySubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}laundry.jpg"),
                parentCategory);

            var kitchenware = CategoryEntity.Create(
                ProductCatalogConstants.KitchenwareSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}kitchenware.jpg"),
                parentCategory);

            await _dbContext.Categories.AddRangeAsync(cleaning, laundry, kitchenware).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
