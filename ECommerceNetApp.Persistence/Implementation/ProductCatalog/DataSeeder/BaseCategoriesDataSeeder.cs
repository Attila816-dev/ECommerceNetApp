using ECommerceNetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    internal abstract class BaseCategoriesDataSeeder(ProductCatalogDbContext dbContext)
    {
        public ProductCatalogDbContext DbContext => dbContext;

        protected virtual async Task<CategoryEntity> GetCategoryAsync(string categoryName, CancellationToken cancellationToken)
        {
            var category = await DbContext.Categories
                .FirstOrDefaultAsync(c => c.Name == categoryName, cancellationToken: cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException(categoryName + " category not found.");
            return category;
        }

        protected virtual async Task AddCategoryAsync(CategoryEntity category, CancellationToken cancellationToken)
        {
            await DbContext.Categories.AddAsync(category, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
