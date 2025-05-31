using ECommerceNetApp.Domain.Enums;

namespace ECommerceNetApp.Domain.Interfaces
{
    public interface ICurrentUserService
    {
        string? Email { get; }

        UserRole? UserRole { get; }

        bool IsInRole(UserRole role);
    }
}
