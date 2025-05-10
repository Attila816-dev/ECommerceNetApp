using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.Category
{
    public record DeleteCategoryCommand(int Id) : ICommand;
}
