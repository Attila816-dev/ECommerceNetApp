using MediatR;

namespace ECommerceNetApp.Service.Commands.Product
{
    public record CreateProductCommand(
        string Name,
        string? Description,
#pragma warning disable CA1054 // URI-like parameters should not be strings
        string? ImageUrl,
#pragma warning restore CA1054 // URI-like parameters should not be strings
        int CategoryId,
        decimal Price,
        int Amount)
        : IRequest;
}
