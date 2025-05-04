using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceNetApp.Service.Implementation
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(UserEntity user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token, out int userId, out string role)
        {
            userId = default;
            role = string.Empty;

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!);

                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                        ValidateIssuer = true,
                        ValidIssuer = _configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = _configuration["Jwt:Audience"],
                        ClockSkew = TimeSpan.Zero,
                    },
                    out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userIdClaim = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub);
                var roleClaim = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role);

                if (int.TryParse(userIdClaim.Value, CultureInfo.InvariantCulture, out userId))
                {
                    role = roleClaim.Value;
                    return true;
                }
            }
            catch (Exception)
            {
                // Token validation failed
                return false;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return false;
        }
    }
}
