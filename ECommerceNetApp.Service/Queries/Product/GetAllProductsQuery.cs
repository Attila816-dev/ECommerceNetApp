using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Product
{
    public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>;
}
