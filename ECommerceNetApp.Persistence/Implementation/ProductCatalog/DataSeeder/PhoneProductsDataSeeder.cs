using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class PhoneProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.PhonesAndTabletsSubCategoryName, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.PhonesAndTabletsSubCategoryName + " category not found.");
            }

            var tablet = ProductEntity.Create(
                "10\" Android Tablet",
                "10-inch Android tablet with HD display and 32GB storage.",
                ImageInfo.Create($"{ProductCatalogConstants.PhonesAndTabletsSubCategoryName}tablet.jpg"),
                category,
                Money.From(129.99m),
                5);

            await dbContext.Products.AddRangeAsync(tablet).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
