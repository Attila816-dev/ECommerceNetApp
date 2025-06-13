using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class ClothingCategoriesDataSeeder(ProductCatalogDbContext dbContext)
        : BaseCategoriesDataSeeder(dbContext), ICategoryDataSeeder
    {
        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Root.ClothingCategoryName, cancellationToken).ConfigureAwait(false);

            // Clothing subcategories
            var mensClothingCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Clothing.Mens,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}mens.jpg"),
                parentCategory);

            await AddCategoryAsync(mensClothingCategory, cancellationToken).ConfigureAwait(false);

            var womensClothingCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Clothing.Womens,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}womens.jpg"),
                parentCategory);

            await AddCategoryAsync(womensClothingCategory, cancellationToken).ConfigureAwait(false);

            var kidsClothingCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Clothing.Kids,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}kids.jpg"),
                parentCategory);

            await AddCategoryAsync(kidsClothingCategory, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
