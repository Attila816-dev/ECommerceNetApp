using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class HouseholdCategoriesDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : ICategoryDataSeeder
    {
        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await productCatalogUnitOfWork.CategoryRepository.FirstOrDefaultAsync(
                c => c.Name == ProductCatalogConstants.HouseholdCategoryName,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidOperationException(ProductCatalogConstants.HouseholdCategoryName + " category not found.");

            // Household subcategories
            var cleaning = CategoryEntity.Create(
                ProductCatalogConstants.CleaningSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}cleaning.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(cleaning, cancellationToken).ConfigureAwait(false);

            var laundry = CategoryEntity.Create(
                ProductCatalogConstants.LaundrySubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}laundry.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(laundry, cancellationToken).ConfigureAwait(false);

            var kitchenware = CategoryEntity.Create(
                ProductCatalogConstants.KitchenwareSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}kitchenware.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(kitchenware, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
