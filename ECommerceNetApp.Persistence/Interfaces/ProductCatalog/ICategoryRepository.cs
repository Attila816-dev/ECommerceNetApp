using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces.ProductCatalog
{
    public interface ICategoryRepository : IRepository<CategoryEntity, int>
    {
        Task<IEnumerable<CategoryEntity>> GetByParentCategoryIdAsync(int? parentCategoryId, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    }
}
