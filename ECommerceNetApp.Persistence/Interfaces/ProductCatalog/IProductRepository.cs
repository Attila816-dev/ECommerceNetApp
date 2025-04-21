using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces.ProductCatalog
{
    public interface IProductRepository : IRepository<ProductEntity, int>
    {
        Task<IEnumerable<ProductEntity>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken);

        Task<(IEnumerable<ProductEntity> Products, int TotalCount)> GetPaginatedProductsAsync(
            int pageNumber,
            int pageSize,
            int? categoryId,
            CancellationToken cancellationToken);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

        Task<bool> ExistsAnyProductWithCategoryIdAsync(int categoryId, CancellationToken cancellationToken);
    }
}