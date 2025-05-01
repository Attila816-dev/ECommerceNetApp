using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class BakeryProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.BakerySubCategoryName, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.BakerySubCategoryName + " category not found.");
            }

            var bread = ProductEntity.Create(
                "Wholemeal Bread 800g",
                "Freshly baked wholemeal bread, ideal for sandwiches.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}bread.jpg"),
                category,
                Money.From(1.20m),
                30);

            var croissants = ProductEntity.Create(
                "Butter Croissants 4pk",
                "Flaky butter croissants, baked in-store daily.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}croissants.jpg"),
                category,
                Money.From(2.50m),
                25);

            await _dbContext.Products.AddRangeAsync(bread, croissants).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
