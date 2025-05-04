using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using ECommerceNetApp.Service.Interfaces;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.User
{
    public class RegisterUserCommandHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        IPasswordService passwordService)
        : IRequestHandler<RegisterUserCommand, int>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork ?? throw new ArgumentNullException(nameof(productCatalogUnitOfWork));
        private readonly IPasswordService _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));

        public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            string passwordHash = _passwordService.HashPassword(request.Password);

            // Create user entity
            var user = UserEntity.Create(
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName,
                request.Role);

            // Save user
            await _productCatalogUnitOfWork.UserRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            return user.Id;
        }
    }
}
