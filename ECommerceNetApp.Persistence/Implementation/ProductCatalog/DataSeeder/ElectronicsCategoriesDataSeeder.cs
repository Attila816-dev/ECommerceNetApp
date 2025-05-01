using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class ElectronicsCategoriesDataSeeder(ProductCatalogDbContext dbContext) : ICategoryDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var parentCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.ElectronicsCategoryName, cancellationToken).ConfigureAwait(false);
            if (parentCategory == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.ElectronicsCategoryName + " category not found.");
            }

            // Electronics subcategories
            var phones = CategoryEntity.Create(
                ProductCatalogConstants.PhonesAndTabletsSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}phones.jpg"),
                parentCategory);

            var computers = CategoryEntity.Create(
                ProductCatalogConstants.ComputersSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}computers.jpg"),
                parentCategory);

            var appliances = CategoryEntity.Create(
                ProductCatalogConstants.AppliancesSubCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}appliances.jpg"),
                parentCategory);

            await _dbContext.Categories.AddRangeAsync(phones, computers, appliances).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
