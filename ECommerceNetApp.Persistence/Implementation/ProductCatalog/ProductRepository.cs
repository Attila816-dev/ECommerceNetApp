using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal class ProductRepository(ProductCatalogDbContext dbContext) : IProductRepository
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext;

        public async Task<IEnumerable<ProductEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Products
                .Include(p => p.Category)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<ProductEntity>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken)
        {
            return await _dbContext.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<ProductEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbContext.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbContext.Products.AnyAsync(c => c.Id == id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAnyProductWithCategoryIdAsync(int categoryId, CancellationToken cancellationToken)
        {
            return await _dbContext.Products.AnyAsync(c => c.CategoryId == categoryId, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddAsync(ProductEntity product, CancellationToken cancellationToken)
        {
            await _dbContext.Products.AddAsync(product, cancellationToken).ConfigureAwait(false);
        }

        public void Update(ProductEntity product)
        {
            _dbContext.Products.Update(product);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products.FindAsync([id], cancellationToken).ConfigureAwait(false);
            if (product != null)
            {
                product.MarkAsDeleted();
                _dbContext.Products.Remove(product);
            }
        }
    }
}
