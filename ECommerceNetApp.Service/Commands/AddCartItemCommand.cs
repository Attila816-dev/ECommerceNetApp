using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Commands
{
    public record AddCartItemCommand(string CartId, CartItemDto Item);
}
