using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces.ProductCatalog
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken cancellationToken);

        Task<IEnumerable<CategoryEntity>> GetByParentCategoryIdAsync(int? parentCategoryId, CancellationToken cancellationToken);

        Task<CategoryEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task AddAsync(CategoryEntity category, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

        void Update(CategoryEntity category);

        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
