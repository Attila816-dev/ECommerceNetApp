using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Product
{
    public record GetProductsByCategoryQuery(int CategoryId) : IRequest<IEnumerable<ProductDto>>;
}
