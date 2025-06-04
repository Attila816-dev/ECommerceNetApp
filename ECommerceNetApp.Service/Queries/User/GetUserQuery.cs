using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Queries.User
{
    public record GetUserQuery(string Email) : IQuery<UserDto?>;
}
