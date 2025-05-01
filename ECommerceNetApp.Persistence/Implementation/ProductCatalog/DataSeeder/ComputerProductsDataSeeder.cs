using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class ComputerProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.ComputersSubCategoryName, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.ComputersSubCategoryName + " category not found.");
            }

            var laptop = ProductEntity.Create(
                "15.6\" Laptop",
                "15.6-inch laptop with Intel Core i5, 8GB RAM, and 256GB SSD.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}laptop.jpg"),
                category,
                Money.From(499.99m),
                3);

            await _dbContext.Products.AddRangeAsync(laptop).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
