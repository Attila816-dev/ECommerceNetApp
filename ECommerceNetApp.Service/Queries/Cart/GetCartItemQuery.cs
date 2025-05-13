using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Queries.Cart
{
    public record GetCartItemQuery(string CartId, int ItemId) : IQuery<CartItemDto?>;
}
