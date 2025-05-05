using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.Cart
{
    public record GetCartQuery(string CartId) : IRequest<CartDto?>;
}
