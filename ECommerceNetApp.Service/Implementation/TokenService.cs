using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommerceNetApp.Domain.Authorization;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceNetApp.Service.Implementation
{
    public class TokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
    {
        private const string TokenTypeClaim = "token_type";

        private readonly JwtOptions _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));

        public string GenerateJwtToken(UserEntity user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return GenerateToken(user, TokenType.Access);
        }

        public string GenerateRefreshToken(UserEntity user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return GenerateToken(user, TokenType.Refresh);
        }

#pragma warning disable CA1031 // Do not catch general exception types
        public TokenValidationResultDto ValidateRefreshToken(string refreshToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidAudience = _jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(_jwtOptions.GetSecretKeyBytes()),
                    ClockSkew = TimeSpan.Zero,
                };

                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out var validatedToken);

                // Verify it's a refresh token
                var tokenTypeClaim = principal.FindFirst(TokenTypeClaim)?.Value;
                if (tokenTypeClaim != TokenType.Refresh.ToString())
                {
                    return new TokenValidationResultDto { IsValid = false, Error = "Invalid token type" };
                }

                var email = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                var fullName = principal.FindFirst(ClaimTypes.Name)?.Value;

                return new TokenValidationResultDto
                {
                    IsValid = true,
                    Email = email,
                    Role = role,
                    FullName = fullName,
                };
            }
            catch (Exception ex)
            {
                return new TokenValidationResultDto { IsValid = false, Error = ex.Message };
            }
        }
#pragma warning restore CA1031 // Do not catch general exception types

        private string GenerateToken(UserEntity user, TokenType tokenType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(_jwtOptions.GetSecretKeyBytes()),
                SecurityAlgorithms.HmacSha256Signature);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(TokenTypeClaim, tokenType.ToString()),
            };

            var expirationHours = tokenType == TokenType.Access
                ? _jwtOptions.ExpirationHours
                : _jwtOptions.RefreshTokenExpirationHours;

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials);

            return tokenHandler.WriteToken(tokenDescriptor);
        }
    }
}
