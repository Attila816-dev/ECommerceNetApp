using ECommerceNetApp.Service.DTO;
using MediatR;

namespace ECommerceNetApp.Service.Queries.User
{
    public record GetUserQuery(string Email) : IRequest<UserDto?>;
}
