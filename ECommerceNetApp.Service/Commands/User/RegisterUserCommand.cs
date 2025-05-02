using MediatR;

namespace ECommerceNetApp.Service.Commands.User
{
    public record RegisterUserCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName)
        : IRequest<int>;
}
