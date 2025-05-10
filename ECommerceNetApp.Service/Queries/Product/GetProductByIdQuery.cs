using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Queries.Product
{
    public record GetProductByIdQuery(int Id) : IQuery<ProductDto?>;
}
