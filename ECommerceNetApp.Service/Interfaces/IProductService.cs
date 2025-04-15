using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Product;

namespace ECommerceNetApp.Service.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(GetAllProductsQuery query);

        Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(GetProductsByCategoryQuery query);

        Task<ProductDto?> GetProductByIdAsync(GetProductByIdQuery query);

        Task<ProductDto> AddProductAsync(CreateProductCommand command);

        Task UpdateProductAsync(UpdateProductCommand command);

        Task DeleteProductAsync(DeleteProductCommand command);
    }
}
