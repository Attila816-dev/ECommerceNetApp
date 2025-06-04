using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(UserEntity user);

        string GenerateRefreshToken(UserEntity user);

        TokenValidationResultDto ValidateRefreshToken(string refreshToken);
    }
}
