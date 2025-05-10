using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.Cart
{
    public record RemoveCartItemCommand(string CartId, int ItemId) : ICommand;
}
