using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Persistence.Implementation
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ProductCatalogDbContext _dbContext;

        public CategoryRepository(ProductCatalogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbContext.Categories
                .Include(c => c.ParentCategory)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _dbContext.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Category>> GetByParentCategoryIdAsync(int? parentCategoryId)
        {
            return await _dbContext.Categories
                .Include(c => c.ParentCategory)
                .Where(c => (parentCategoryId == null && c.ParentCategoryId == null) || (parentCategoryId != null && c.ParentCategoryId == parentCategoryId))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Categories.AnyAsync(c => c.Id == id).ConfigureAwait(false);
        }

        public async Task<Category> AddAsync(Category category)
        {
            await _dbContext.Categories.AddAsync(category).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id).ConfigureAwait(false);
            if (category != null)
            {
                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
