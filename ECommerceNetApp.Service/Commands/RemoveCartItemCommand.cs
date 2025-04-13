using MediatR;

namespace ECommerceNetApp.Service.Commands
{
    public record RemoveCartItemCommand(string CartId, int ItemId) : IRequest;
}
