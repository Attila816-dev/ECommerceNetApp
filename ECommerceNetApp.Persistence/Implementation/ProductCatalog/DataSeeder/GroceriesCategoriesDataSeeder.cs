using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class GroceriesCategoriesDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : BaseCategoriesDataSeeder(productCatalogUnitOfWork), ICategoryDataSeeder
    {
        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Root.Groceries, cancellationToken).ConfigureAwait(false);

            // Groceries subcategories
            var fruitsCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Groceries.FruitsAndVegetables,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}fruits.jpg"),
                parentCategory);

            await AddCategoryAsync(fruitsCategory, cancellationToken).ConfigureAwait(false);

            var dairyCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Groceries.Dairy,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}dairy.jpg"),
                parentCategory);

            await AddCategoryAsync(dairyCategory, cancellationToken).ConfigureAwait(false);

            var bakeryCatgory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Groceries.Bakery,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}bakery.jpg"),
                parentCategory);

            await AddCategoryAsync(bakeryCatgory, cancellationToken).ConfigureAwait(false);

            var meatCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Groceries.Meat,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}meat.jpg"),
                parentCategory);

            await AddCategoryAsync(meatCategory, cancellationToken).ConfigureAwait(false);

            var drinksCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Groceries.Drinks,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}drinks.jpg"),
                parentCategory);

            await AddCategoryAsync(drinksCategory, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
