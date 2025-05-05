using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.User
{
    public class LoginUserCommandHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        IPasswordService passwordService,
        ITokenService tokenService)
        : IRequestHandler<LoginUserCommand, LoginUserCommandResponse>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork ?? throw new ArgumentNullException(nameof(productCatalogUnitOfWork));
        private readonly IPasswordService _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

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
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return LoginUserCommandResponse.Failed("Invalid email or password");
            }

            // Update last login date
            user.UpdateLoginDate();
            _productCatalogUnitOfWork.UserRepository.Update(user);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

            var token = _tokenService.GenerateJwtToken(user);
            return LoginUserCommandResponse.Successful(token);
        }
    }
}
