using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal class ProductRepository(ProductCatalogDbContext dbContext)
        : BaseRepository<ProductEntity, int>(dbContext), IProductRepository
    {
        public override async Task<IEnumerable<ProductEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await DbSet
                .Include(p => p.Category)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<ProductEntity>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken)
        {
            return await DbSet
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public override async Task<ProductEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await DbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsAnyProductWithCategoryIdAsync(int categoryId, CancellationToken cancellationToken)
        {
            return await DbSet.AnyAsync(c => c.CategoryId == categoryId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<(IEnumerable<ProductEntity> Products, int TotalCount)> GetPaginatedProductsAsync(
            int pageNumber,
            int pageSize,
            int? categoryId,
            CancellationToken cancellationToken)
        {
            IQueryable<ProductEntity> query = DbSet.Include(p => p.Category);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return (products, totalCount);
        }
    }
}
