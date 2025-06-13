using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.User
{
    public class LoginUserCommandHandler(
        ProductCatalogDbContext dbContext,
        IPasswordService passwordService,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
        : ICommandHandler<LoginUserCommand, LoginUserCommandResponse>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly IPasswordService _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        private readonly JwtOptions _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));

        public async Task<LoginUserCommandResponse> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));

            // Get user by email
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToUpper() == command.Email.ToUpper(), cancellationToken).ConfigureAwait(false);
            if (user == null)
            {
                return LoginUserCommandResponse.Failed("Invalid email or password");
            }

            // Verify password
            if (!_passwordService.VerifyPassword(command.Password, user.PasswordHash))
            {
                return LoginUserCommandResponse.Failed("Invalid email or password");
            }

            // Update last login date
            user.UpdateLoginDate();
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);
            var idToken = _tokenService.GenerateIdToken(user);
            return LoginUserCommandResponse.Successful(accessToken, refreshToken, idToken, _jwtOptions.ExpirationHours);
        }
    }
}
