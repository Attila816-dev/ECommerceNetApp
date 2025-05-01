using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class GroceriesCategoriesDataSeeder(ProductCatalogDbContext dbContext) : ICategoryDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.GroceriesCategoryName, cancellationToken).ConfigureAwait(false);
            if (parentCategory == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.GroceriesCategoryName + " category not found.");
            }

            // Groceries subcategories
            var fruits = CategoryEntity.Create(
                ProductCatalogConstants.FruitsAndVegetablesSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}fruits.jpg"),
                parentCategory);

            var dairy = CategoryEntity.Create(
                ProductCatalogConstants.DairySubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}dairy.jpg"),
                parentCategory);

            var bakery = CategoryEntity.Create(
                ProductCatalogConstants.BakerySubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}bakery.jpg"),
                parentCategory);

            var meat = CategoryEntity.Create(
                ProductCatalogConstants.MeatSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}meat.jpg"),
                parentCategory);

            var drinks = CategoryEntity.Create(
                ProductCatalogConstants.DrinksSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}drinks.jpg"),
                parentCategory);

            await _dbContext.Categories.AddRangeAsync(fruits, dairy, bakery, meat, drinks).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
