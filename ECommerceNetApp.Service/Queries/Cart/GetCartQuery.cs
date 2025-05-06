using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Queries.Cart
{
    public record GetCartQuery(string CartId) : IQuery<CartDto?>;
}
