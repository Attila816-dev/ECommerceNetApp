using ECommerceNetApp.Domain;
using ECommerceNetApp.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductCatalogDbContext _dbContext;

        public ProductRepository(ProductCatalogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbContext.Products
                .Include(p => p.Category)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId)
        {
            return await _dbContext.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbContext.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<Product> AddAsync(Product product)
        {
            await _dbContext.Products.AddAsync(product).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id).ConfigureAwait(false);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
