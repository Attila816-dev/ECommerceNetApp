using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class FruitProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.FruitsAndVegetablesSubCategoryName, cancellationToken).ConfigureAwait(false);

            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.FruitsAndVegetablesSubCategoryName + " category not found.");
            }

            var apples = ProductEntity.Create(
                "Gala Apples 1kg",
                "Sweet and crisp Gala apples, perfect for snacking or cooking.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}apples.jpg"),
                category,
                Money.From(2.50m),
                100);

            var bananas = ProductEntity.Create(
                "Bananas 5pk",
                "Ripe and ready-to-eat bananas, perfect for breakfast or as a healthy snack.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}bananas.jpg"),
                category,
                Money.From(1.95m),
                150);

            var oranges = ProductEntity.Create(
                "Navel Oranges 4pk",
                "Sweet and juicy Navel oranges, rich in vitamin C.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}oranges.jpg"),
                category,
                Money.From(3.25m),
                80);

            await _dbContext.Products.AddRangeAsync(apples, bananas, oranges).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
