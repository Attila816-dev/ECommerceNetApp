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

        public string GenerateAccessToken(UserEntity user)
        {
            ArgumentNullException.ThrowIfNull(user);
            return GenerateToken(user, TokenType.Access);
        }

        public string GenerateRefreshToken(UserEntity user)
        {
            ArgumentNullException.ThrowIfNull(user);
            return GenerateToken(user, TokenType.Refresh);
        }

        public string GenerateIdToken(UserEntity user)
        {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return GenerateToken(user, TokenType.Id);
        }

        public TokenValidationResultDto ValidateToken(string token, TokenType expectedTokenType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new TokenValidationResultDto { IsValid = false, Error = "Token is null or empty" };
                }

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

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // Verify it's the expected token type
                var tokenTypeClaim = principal.FindFirst(TokenTypeClaim)?.Value;
                if (tokenTypeClaim != expectedTokenType.ToString())
                {
                    return new TokenValidationResultDto { IsValid = false, Error = $"Invalid token type. Expected: {expectedTokenType}, Actual: {tokenTypeClaim}" };
                }

                var email = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? principal.FindFirst(ClaimTypes.Email)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                var fullName = principal.FindFirst(ClaimTypes.Name)?.Value;
                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                return new TokenValidationResultDto
                {
                    IsValid = true,
                    Email = email,
                    Role = role,
                    FullName = fullName,
                    TokenId = jti,
                    TokenType = expectedTokenType,
                };
            }
            catch (SecurityTokenExpiredException)
            {
                return new TokenValidationResultDto { IsValid = false, Error = "Token has expired" };
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return new TokenValidationResultDto { IsValid = false, Error = "Invalid token signature" };
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                return new TokenValidationResultDto { IsValid = false, Error = "Invalid token issuer" };
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                return new TokenValidationResultDto { IsValid = false, Error = "Invalid token audience" };
            }
            catch (Exception ex)
            {
                return new TokenValidationResultDto { IsValid = false, Error = $"Token validation failed: {ex.Message}" };
            }
        }

        public TokenValidationResultDto ValidateRefreshToken(string refreshToken)
        {
            return ValidateToken(refreshToken, TokenType.Refresh);
        }

        public TokenValidationResultDto ValidateIdToken(string idToken)
        {
            return ValidateToken(idToken, TokenType.Id);
        }

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
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(TokenTypeClaim, tokenType.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
            };

            // Add specific claims based on token type
            switch (tokenType)
            {
                case TokenType.Id:
                    // ID tokens should contain user profile information (OpenID Connect standard)
                    claims.Add(new Claim("given_name", user.FirstName));
                    claims.Add(new Claim("family_name", user.LastName));
                    claims.Add(new Claim("email_verified", "true"));
                    claims.Add(new Claim(JwtRegisteredClaimNames.Aud, _jwtOptions.Audience)); // Audience is important for ID tokens
                    break;
                case TokenType.Access:
                    // Access tokens can contain permissions/scopes
                    // These could be added based on user role
                    claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
                    break;
                case TokenType.Refresh:
                    // Refresh tokens should be minimal - only essential claims
                    break;
            }

            var expirationHours = tokenType switch
            {
                TokenType.Access => _jwtOptions.ExpirationHours,
                TokenType.Refresh => _jwtOptions.RefreshTokenExpirationHours,
                TokenType.Id => _jwtOptions.ExpirationHours, // ID tokens typically have same expiration as access tokens
                _ => _jwtOptions.ExpirationHours,
            };

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
