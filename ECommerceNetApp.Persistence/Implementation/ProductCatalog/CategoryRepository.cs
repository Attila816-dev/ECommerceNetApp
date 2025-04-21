using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal class CategoryRepository(
        ProductCatalogDbContext dbContext)
        : BaseRepository<CategoryEntity, int>(dbContext), ICategoryRepository
    {
        public Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return GetAllAsync(
                include: query => query.Include(c => c.ParentCategory),
                cancellationToken: cancellationToken);
        }

        public Task<CategoryEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return GetByIdAsync(id, include: query => query.Include(c => c.ParentCategory), cancellationToken);
        }

        public Task<IEnumerable<CategoryEntity>> GetByParentCategoryIdAsync(int? parentCategoryId, CancellationToken cancellationToken)
        {
            return GetAllAsync(
                filter: c => (parentCategoryId == null && c.ParentCategoryId == null) || (parentCategoryId != null && c.ParentCategoryId == parentCategoryId),
                include: query => query.Include(c => c.ParentCategory),
                cancellationToken: cancellationToken);
        }

        public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
        {
            return ExistsAsync(p => p.Id == id, cancellationToken);
        }
    }
}
