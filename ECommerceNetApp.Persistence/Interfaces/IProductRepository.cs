using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken);

        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken);

        Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

        Task<Product> AddAsync(Product product, CancellationToken cancellationToken);

        Task UpdateAsync(Product product, CancellationToken cancellationToken);

        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}