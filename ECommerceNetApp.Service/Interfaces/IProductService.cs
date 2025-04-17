namespace ECommerceNetApp.Service.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();

        Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId);

        Task<ProductDto?> GetProductByIdAsync(int id);

        Task<ProductDto> AddProductAsync(ProductDto productDto);

        Task UpdateProductAsync(ProductDto productDto);

        Task DeleteProductAsync(int id);
    }
}
