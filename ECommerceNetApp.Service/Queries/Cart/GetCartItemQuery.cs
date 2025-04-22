using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Cart
{
    public record GetCartItemQuery(string CartId, int ItemId) : IRequest<CartItemDto?>;
}
