using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class ClothingCategoriesDataSeeder(ProductCatalogDbContext dbContext) : ICategoryDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.ClothingCategoryName, cancellationToken).ConfigureAwait(false);
            if (parentCategory == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.ClothingCategoryName + " category not found.");
            }

            // Clothing subcategories
            var mens = CategoryEntity.Create(
                ProductCatalogConstants.MensClothingSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}mens.jpg"),
                parentCategory);

            var womens = CategoryEntity.Create(
                ProductCatalogConstants.WomensClothingSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}womens.jpg"),
                parentCategory);

            var kids = CategoryEntity.Create(
                ProductCatalogConstants.KidsClothingSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}kids.jpg"),
                parentCategory);

            await _dbContext.Categories.AddRangeAsync(mens, womens, kids).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
