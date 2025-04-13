using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Commands
{
    public record AddCartItemCommand(string CartId, CartItemDto Item) : IRequest;
}
