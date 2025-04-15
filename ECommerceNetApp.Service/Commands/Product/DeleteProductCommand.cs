using MediatR;

namespace ECommerceNetApp.Service.Commands.Product
{
    public record DeleteProductCommand(int Id) : IRequest;
}
