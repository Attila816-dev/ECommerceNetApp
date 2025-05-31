using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.User
{
    public class RefreshTokenCommandHandler(
        ProductCatalogDbContext dbContext,
        ITokenService tokenService)
        : ICommandHandler<RefreshTokenCommand, RefreshTokenCommandResponse>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

        public async Task<RefreshTokenCommandResponse> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            var validationResult = _tokenService.ValidateRefreshToken(command.RefreshToken);

            if (!validationResult.IsValid)
            {
                return RefreshTokenCommandResponse.Failed(validationResult.Error!);
            }

            // Get user by email
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToUpper() == validationResult.Email!.ToUpper(), cancellationToken).ConfigureAwait(false);
            if (user == null)
            {
                return RefreshTokenCommandResponse.Failed("Invalid email or password");
            }

            var newAccessToken = _tokenService.GenerateJwtToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user);
            return RefreshTokenCommandResponse.Successful(newAccessToken, newRefreshToken);
        }
    }
}
