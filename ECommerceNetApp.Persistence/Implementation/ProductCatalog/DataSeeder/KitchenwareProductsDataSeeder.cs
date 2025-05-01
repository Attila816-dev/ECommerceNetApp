using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class KitchenwareProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.KitchenwareSubCategoryName, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.KitchenwareSubCategoryName + " category not found.");
            }

            var pan = ProductEntity.Create(
                "Non-Stick Frying Pan 28cm",
                "Durable non-stick frying pan, suitable for all hob types.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}pan.jpg"),
                category,
                Money.From(15.99m),
                10);

            await _dbContext.Products.AddRangeAsync(pan).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
