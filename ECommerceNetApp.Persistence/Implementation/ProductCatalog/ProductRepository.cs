using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductCatalogDbContext _dbContext;
        private readonly IDomainEventService _domainEventService;

        public ProductRepository(ProductCatalogDbContext dbContext, IDomainEventService domainEventService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _domainEventService = domainEventService ?? throw new ArgumentNullException(nameof(domainEventService));
        }

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

        public async Task AddAsync(ProductEntity product, CancellationToken cancellationToken)
        {
            await _dbContext.Products.AddAsync(product, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await _domainEventService.PublishEventsAsync(product, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(ProductEntity product, CancellationToken cancellationToken)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await _domainEventService.PublishEventsAsync(product, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products.FindAsync([id], cancellationToken: cancellationToken).ConfigureAwait(false);
            if (product != null)
            {
                product.MarkAsDeleted();
                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await _domainEventService.PublishEventsAsync(product, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
