using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductEntity>> GetAllAsync(CancellationToken cancellationToken);

        Task<IEnumerable<ProductEntity>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken);

        Task<ProductEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

        Task<bool> ExistsAnyProductWithCategoryIdAsync(int categoryId, CancellationToken cancellationToken);

        Task AddAsync(ProductEntity product, CancellationToken cancellationToken);

        Task UpdateAsync(ProductEntity product, CancellationToken cancellationToken);

        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}