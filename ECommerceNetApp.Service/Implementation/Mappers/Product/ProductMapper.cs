using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;

namespace ECommerceNetApp.Service.Implementation.Mappers.Product
{
    public class ProductMapper : IProductMapper
    {
        public ProductEntity MapToEntity(CreateProductCommand command, CategoryEntity? category)
        {
            ArgumentNullException.ThrowIfNull(command);

            var product = ProductEntity.Create(
                command.Name,
                command.Description,
                command.ImageUrl != null ? ImageInfo.Create(command.ImageUrl) : null,
                category!,
                Money.Create(command.Price, command.Currency),
                command.Amount);
            return product;
        }

        public ProductDto MapToProductDto(ProductEntity product)
        {
            ArgumentNullException.ThrowIfNull(product);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.Image?.Url,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Price = product.Price.Amount,
                Currency = product.Price.Currency,
                Amount = product.Amount,
            };
        }
    }
}
