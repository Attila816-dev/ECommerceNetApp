using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal class ProductRepository(ProductCatalogDbContext dbContext)
        : BaseRepository<ProductEntity, int>(dbContext), IProductRepository
    {
        public Task<IEnumerable<ProductEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return GetAllAsync(
                include: query => query.Include(p => p.Category),
                cancellationToken: cancellationToken);
        }

        public Task<IEnumerable<ProductEntity>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken)
        {
            return GetAllAsync(
                filter: p => p.CategoryId == categoryId,
                include: query => query.Include(p => p.Category),
                cancellationToken: cancellationToken);
        }

        public Task<ProductEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return GetByIdAsync(id, include: query => query.Include(p => p.Category), cancellationToken);
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
