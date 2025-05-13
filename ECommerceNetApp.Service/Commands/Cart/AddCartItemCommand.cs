using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Commands.Cart
{
    public record AddCartItemCommand(string CartId, CartItemDto Item) : ICommand;
}