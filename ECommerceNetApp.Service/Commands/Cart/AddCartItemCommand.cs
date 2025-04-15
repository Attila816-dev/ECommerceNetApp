using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Commands.Cart
{
    public record AddCartItemCommand(string CartId, CartItemDto Item) : IRequest;
}