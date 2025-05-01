using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class GroceriesCategoriesDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : ICategoryDataSeeder
    {
        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await productCatalogUnitOfWork.CategoryRepository.FirstOrDefaultAsync(
                c => c.Name == ProductCatalogConstants.GroceriesCategoryName, cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidOperationException(ProductCatalogConstants.GroceriesCategoryName + " category not found.");

            // Groceries subcategories
            var fruitsCategory = CategoryEntity.Create(
                ProductCatalogConstants.FruitsAndVegetablesSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}fruits.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(fruitsCategory, cancellationToken).ConfigureAwait(false);

            var dairyCategory = CategoryEntity.Create(
                ProductCatalogConstants.DairySubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}dairy.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(dairyCategory, cancellationToken).ConfigureAwait(false);

            var bakeryCatgory = CategoryEntity.Create(
                ProductCatalogConstants.BakerySubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}bakery.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(bakeryCatgory, cancellationToken).ConfigureAwait(false);

            var meatCategory = CategoryEntity.Create(
                ProductCatalogConstants.MeatSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}meat.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(meatCategory, cancellationToken).ConfigureAwait(false);

            var drinksCategory = CategoryEntity.Create(
                ProductCatalogConstants.DrinksSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}drinks.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(drinksCategory, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
