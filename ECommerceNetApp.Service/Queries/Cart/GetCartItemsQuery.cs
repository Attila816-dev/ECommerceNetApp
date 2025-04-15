using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Cart
{
    public record GetCartItemsQuery(string CartId) : IRequest<List<CartItemDto>?>;
}
