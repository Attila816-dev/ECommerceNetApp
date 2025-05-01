using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal class LaundryProductsDataSeeder(ProductCatalogDbContext dbContext) : IProductDataSeeder
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == ProductCatalogConstants.LaundrySubCategoryName, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                throw new InvalidOperationException(ProductCatalogConstants.LaundrySubCategoryName + " category not found.");
            }

            var detergent = ProductEntity.Create(
                "Laundry Detergent 2L",
                "Concentrated laundry detergent for fresh, clean clothes.",
                ImageInfo.Create($"{ProductCatalogConstants.ProductImagePrefix}detergent.jpg"),
                category,
                Money.From(5.95m),
                25);

            await dbContext.Products.AddRangeAsync(detergent).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
