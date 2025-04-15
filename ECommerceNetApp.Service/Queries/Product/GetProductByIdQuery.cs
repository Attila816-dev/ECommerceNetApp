using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Product
{
    public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
}
