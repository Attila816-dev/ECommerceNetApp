using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class DrinkProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.DrinksSubCategoryName, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.DrinksSubCategoryName + " category not found.");
            }

            var water = ProductEntity.Create(
                "Spring Water 6x1.5L",
                "Natural spring water, refreshing and pure.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}water.jpg"),
                category,
                Money.From(2.75m),
                40);

            var cola = ProductEntity.Create(
                "Cola 2L",
                "Classic cola soft drink.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}cola.jpg"),
                category,
                Money.From(1.95m),
                35);

            await _dbContext.Products.AddRangeAsync(water, cola).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
