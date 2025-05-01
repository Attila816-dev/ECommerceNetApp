using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class CleaningProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.CleaningSubCategoryName, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.CleaningSubCategoryName + " category not found.");
            }

            var allPurpose = ProductEntity.Create(
                "All-Purpose Cleaner 1L",
                "Powerful all-purpose cleaning solution for all surfaces.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}cleaner.jpg"),
                category,
                Money.From(2.25m),
                50);

            var dishwasher = ProductEntity.Create(
                "Dishwasher Tablets 40pk",
                "Powerful dishwasher tablets for sparkling clean dishes.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}dishwasher.jpg"),
                category,
                Money.From(6.50m),
                30);

            await dbContext.Products.AddRangeAsync(allPurpose, dishwasher).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
