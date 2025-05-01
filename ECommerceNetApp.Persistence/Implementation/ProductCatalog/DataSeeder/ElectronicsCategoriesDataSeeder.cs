using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class ElectronicsCategoriesDataSeeder(IProductCatalogUnitOfWork productCatalogUnitOfWork) : ICategoryDataSeeder
    {
        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await productCatalogUnitOfWork.CategoryRepository
                .FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.ElectronicsCategoryName, cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? throw new InvalidOperationException(ProductCatalogConstants.ElectronicsCategoryName + " category not found.");

            // Electronics subcategories
            var phones = CategoryEntity.Create(
                ProductCatalogConstants.PhonesAndTabletsSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}phones.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(phones, cancellationToken).ConfigureAwait(false);

            var computers = CategoryEntity.Create(
                ProductCatalogConstants.ComputersSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}computers.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(computers, cancellationToken).ConfigureAwait(false);

            var appliances = CategoryEntity.Create(
                ProductCatalogConstants.AppliancesSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}appliances.jpg"),
                parentCategory);

            await productCatalogUnitOfWork.CategoryRepository.AddAsync(appliances, cancellationToken).ConfigureAwait(false);
            await productCatalogUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
