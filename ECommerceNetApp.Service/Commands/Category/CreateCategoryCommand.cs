using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.Category
{
    public record CreateCategoryCommand(
        string Name,
#pragma warning disable CA1054 // URI-like parameters should not be strings
        string? ImageUrl,
#pragma warning restore CA1054 // URI-like parameters should not be strings
        int? ParentCategoryId)
        : ICommand<int>;
}
