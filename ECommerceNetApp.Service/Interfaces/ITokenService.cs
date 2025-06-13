using ECommerceNetApp.Domain.Authorization;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(UserEntity user);

        string GenerateIdToken(UserEntity user);

        string GenerateRefreshToken(UserEntity user);

        TokenValidationResultDto ValidateIdToken(string idToken);

        TokenValidationResultDto ValidateRefreshToken(string refreshToken);

        TokenValidationResultDto ValidateToken(string token, TokenType expectedTokenType);
    }
}
