using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Interfaces.Mappers.Product
{
    public interface IProductMapper
    {
        ProductEntity MapToEntity(CreateProductCommand command, CategoryEntity? category);

        ProductDto MapToProductDto(ProductEntity product);
    }
}
