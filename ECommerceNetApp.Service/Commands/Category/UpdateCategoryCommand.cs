using MediatR;

namespace ECommerceNetApp.Service.Commands.Category
{
    public record UpdateCategoryCommand(
        int Id,
        string Name,
#pragma warning disable CA1054 // URI-like parameters should not be strings
        string? ImageUrl,
#pragma warning restore CA1054 // URI-like parameters should not be strings
        int? ParentCategoryId) : IRequest;
}