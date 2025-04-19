using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal class CategoryRepository(
        ProductCatalogDbContext dbContext) : ICategoryRepository
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public async Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Categories
                .Include(c => c.ParentCategory)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<CategoryEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbContext.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<CategoryEntity>> GetByParentCategoryIdAsync(int? parentCategoryId, CancellationToken cancellationToken)
        {
            return await _dbContext.Categories
                .Include(c => c.ParentCategory)
                .Where(c => (parentCategoryId == null && c.ParentCategoryId == null) || (parentCategoryId != null && c.ParentCategoryId == parentCategoryId))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbContext.Categories.AnyAsync(c => c.Id == id, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddAsync(CategoryEntity category, CancellationToken cancellationToken)
        {
            await _dbContext.Categories.AddAsync(category, cancellationToken).ConfigureAwait(false);
        }

        public void Update(CategoryEntity category)
        {
            _dbContext.Categories.Update(category);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var category = await _dbContext.Categories.FindAsync([id], cancellationToken).ConfigureAwait(false);
            if (category != null)
            {
                category.MarkAsDeleted();
                _dbContext.Categories.Remove(category);
            }
        }
    }
}
