using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using MediatR;
using BC = BCrypt.Net.BCrypt;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.User
{
    public class RegisterUserCommandHandler(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        : IRequestHandler<RegisterUserCommand, int>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork ?? throw new ArgumentNullException(nameof(productCatalogUnitOfWork));

        public async Task<int> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            string passwordHash = BC.HashPassword(request.Password);

            // Create user entity
            var user = UserEntity.Create(
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName);

            // Save user
            await _productCatalogUnitOfWork.UserRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            return user.Id;
        }
    }
}
