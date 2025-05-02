using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class ElectronicsCategoriesDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : BaseCategoriesDataSeeder(productCatalogUnitOfWork), ICategoryDataSeeder
    {
        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await GetCategoryAsync(ProductCatalogConstants.CategoryNames.Root.Electronics, cancellationToken).ConfigureAwait(false);

            // Electronics subcategories
            var phones = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Electronics.PhonesAndTablets,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}phones.jpg"),
                parentCategory);

            await AddCategoryAsync(phones, cancellationToken).ConfigureAwait(false);

            var computers = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Electronics.Computers,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}computers.jpg"),
                parentCategory);

            await AddCategoryAsync(computers, cancellationToken).ConfigureAwait(false);

            var appliances = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Electronics.Appliances,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}appliances.jpg"),
                parentCategory);

            await AddCategoryAsync(appliances, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
