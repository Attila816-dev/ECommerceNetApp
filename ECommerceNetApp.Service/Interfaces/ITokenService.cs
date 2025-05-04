using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Service.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(UserEntity user);

        bool ValidateToken(string token, out int userId, out string role);
    }
}
