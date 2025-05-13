using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.Product
{
    public record UpdateProductCommand(
        int Id,
        string Name,
        string? Description,
#pragma warning disable CA1054 // URI-like parameters should not be strings
        string? ImageUrl,
#pragma warning restore CA1054 // URI-like parameters should not be strings
        int CategoryId,
        decimal Price,
        string? Currency,
        int Amount) : ICommand;
}
