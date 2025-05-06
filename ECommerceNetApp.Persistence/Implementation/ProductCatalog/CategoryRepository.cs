using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal class CategoryRepository(
        ProductCatalogDbContext dbContext)
        : BaseRepository<CategoryEntity, int>(dbContext), ICategoryRepository
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public override async Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Categories
                .Include(c => c.ParentCategory)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public override async Task<CategoryEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
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
    }
}
