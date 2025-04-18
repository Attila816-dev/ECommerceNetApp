﻿using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ProductCatalogDbContext _dbContext;
        private readonly IDomainEventService _domainEventService;

        public CategoryRepository(ProductCatalogDbContext dbContext, IDomainEventService domainEventService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _domainEventService = domainEventService ?? throw new ArgumentNullException(nameof(domainEventService));
        }

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
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await _domainEventService.PublishEventsAsync(category, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(CategoryEntity category, CancellationToken cancellationToken)
        {
            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await _domainEventService.PublishEventsAsync(category, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var category = await _dbContext.Categories.FindAsync([id], cancellationToken: cancellationToken).ConfigureAwait(false);
            if (category != null)
            {
                category.MarkAsDeleted();
                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await _domainEventService.PublishEventsAsync(category, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
