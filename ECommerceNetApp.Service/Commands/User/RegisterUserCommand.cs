using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.User
{
    public record RegisterUserCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        UserRole Role)
        : ICommand<int>;
}
