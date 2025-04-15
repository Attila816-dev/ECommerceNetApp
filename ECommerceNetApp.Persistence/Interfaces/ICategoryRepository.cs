using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();

        Task<IEnumerable<Category>> GetByParentCategoryIdAsync(int? parentCategoryId);

        Task<Category?> GetByIdAsync(int id);

        Task<Category> AddAsync(Category category);

        Task<bool> ExistsAsync(int id);

        Task UpdateAsync(Category category);

        Task DeleteAsync(int id);
    }
}
