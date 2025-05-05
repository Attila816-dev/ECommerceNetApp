using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Domain.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(UserEntity user);
    }
}
