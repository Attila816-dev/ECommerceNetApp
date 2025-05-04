using ECommerceNetApp.Domain.Enums;
using MediatR;

namespace ECommerceNetApp.Service.Commands.User
{
    public record RegisterUserCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        UserRole Role)
        : IRequest<int>;
}
