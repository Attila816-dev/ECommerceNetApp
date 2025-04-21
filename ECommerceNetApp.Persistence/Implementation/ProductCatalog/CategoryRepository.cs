using System.Linq.Expressions;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal class CategoryRepository(
        ProductCatalogDbContext dbContext)
        : BaseRepository<CategoryEntity, int>(dbContext), ICategoryRepository
    {
        public override Task<IEnumerable<CategoryEntity>> GetAllAsync(Expression<Func<CategoryEntity, bool>>? filter = null, Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>? include = null, CancellationToken cancellationToken = default)
        {
            if (include == null)
            {
                include = query => query.Include(p => p.ParentCategory);
            }

            return base.GetAllAsync(filter, include, cancellationToken);
        }

        public override Task<CategoryEntity?> GetByIdAsync(int id, Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>? include = null, CancellationToken cancellationToken = default)
        {
            if (include == null)
            {
                include = query => query.Include(p => p.ParentCategory);
            }

            return base.GetByIdAsync(id, include, cancellationToken);
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
