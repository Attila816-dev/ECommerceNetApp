using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.User
{
    public class LoginUserCommandHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        IConfiguration configuration)
        : IRequestHandler<LoginUserCommand, LoginUserCommandResponse>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork ?? throw new ArgumentNullException(nameof(productCatalogUnitOfWork));
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        public async Task<LoginUserCommandResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            // Get user by email
            var user = await _productCatalogUnitOfWork.UserRepository.GetUserByEmailAsync(request.Email, cancellationToken).ConfigureAwait(false);
            if (user == null)
            {
                return LoginUserCommandResponse.Failed("Invalid email or password");
            }

            // Verify password
            if (!BC.Verify(request.Password, user.PasswordHash))
            {
                return LoginUserCommandResponse.Failed("Invalid email or password");
            }

            // Update last login date
            user.UpdateLoginDate();
            _productCatalogUnitOfWork.UserRepository.Update(user);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

            // Generate JWT token
            var token = GenerateJwtToken(user);
            return LoginUserCommandResponse.Successful(token);
        }

        private string GenerateJwtToken(Domain.Entities.UserEntity user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
