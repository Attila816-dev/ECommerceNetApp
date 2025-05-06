using System.Linq.Expressions;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.Repository
{
    internal class ProductRepository(ProductCatalogDbContext dbContext)
        : BaseRepository<ProductEntity, int>(dbContext), IProductRepository
    {
        public override Task<IEnumerable<ProductEntity>> GetAllAsync(Expression<Func<ProductEntity, bool>>? filter = null, Func<IQueryable<ProductEntity>, IQueryable<ProductEntity>>? include = null, CancellationToken cancellationToken = default)
        {
            if (include == null)
            {
                include = query => query.Include(p => p.Category);
            }

            return base.GetAllAsync(filter, include, cancellationToken);
        }

        public override Task<ProductEntity?> GetByIdAsync(int id, Func<IQueryable<ProductEntity>, IQueryable<ProductEntity>>? include = null, CancellationToken cancellationToken = default)
        {
            if (include == null)
            {
                include = query => query.Include(p => p.Category);
            }

            return base.GetByIdAsync(id, include, cancellationToken);
        }

        public Task<IEnumerable<ProductEntity>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken)
        {
            return GetAllAsync(
                filter: p => p.CategoryId == categoryId,
                include: query => query.Include(p => p.Category),
                cancellationToken: cancellationToken);
        }

        public Task<(IEnumerable<ProductEntity> Products, int TotalCount)> GetPaginatedProductsAsync(
            int pageNumber,
            int pageSize,
            int? categoryId,
            CancellationToken cancellationToken)
        {
            return GetPaginatedEntitiesAsync(
                pageNumber,
                pageSize,
                filter: categoryId.HasValue ? p => p.CategoryId == categoryId.Value : null,
                include: query => query.Include(p => p.Category),
                cancellationToken);
        }

        public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
        {
            return ExistsAsync(p => p.Id == id, cancellationToken);
        }

        public Task<bool> ExistsAnyProductWithCategoryIdAsync(int categoryId, CancellationToken cancellationToken)
        {
            return ExistsAsync(c => c.CategoryId == categoryId, cancellationToken);
        }
    }
}
