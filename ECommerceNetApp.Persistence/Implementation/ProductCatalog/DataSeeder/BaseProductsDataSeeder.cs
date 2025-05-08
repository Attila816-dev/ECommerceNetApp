using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal abstract class BaseProductsDataSeeder(ProductCatalogDbContext dbContext)
    {
        public ProductCatalogDbContext DbContext => dbContext;

        protected virtual async Task<CategoryEntity> GetCategoryAsync(string categoryName, CancellationToken cancellationToken)
        {
            var category = await DbContext.Categories
                .FirstOrDefaultAsync(c => c.Name == categoryName, cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException(categoryName + " category not found.");
            return category;
        }

        protected virtual async Task AddProductAsync(ProductEntity product, CancellationToken cancellationToken)
        {
            await DbContext.Products.AddAsync(product, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
