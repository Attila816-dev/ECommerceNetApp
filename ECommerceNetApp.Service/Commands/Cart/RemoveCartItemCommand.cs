using MediatR;

namespace ECommerceNetApp.Service.Commands.Cart
{
    public record RemoveCartItemCommand(string CartId, int ItemId) : IRequest;
}
