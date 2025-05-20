using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.Product
{
    public record DeleteProductCommand(int Id) : ICommand;
}
