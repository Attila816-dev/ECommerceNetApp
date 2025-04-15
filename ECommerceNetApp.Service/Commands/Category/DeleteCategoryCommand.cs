using MediatR;

namespace ECommerceNetApp.Service.Commands.Category
{
    public record DeleteCategoryCommand(int Id) : IRequest;
}
