using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Queries.Cart
{
    public record GetCartItemsQuery(string CartId) : IQuery<List<CartItemDto>?>;
}
