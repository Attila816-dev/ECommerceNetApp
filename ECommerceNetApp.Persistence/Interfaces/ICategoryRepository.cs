using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken);

        Task<IEnumerable<Category>> GetByParentCategoryIdAsync(int? parentCategoryId, CancellationToken cancellationToken);

        Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task AddAsync(Category category, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

        Task UpdateAsync(Category category, CancellationToken cancellationToken);

        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
