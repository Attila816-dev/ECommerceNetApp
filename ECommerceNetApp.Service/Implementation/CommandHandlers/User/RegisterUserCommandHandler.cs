using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.User
{
    public class RegisterUserCommandHandler(
        ProductCatalogDbContext dbContext,
        IPasswordService passwordService)
        : ICommandHandler<RegisterUserCommand, int>
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly IPasswordService _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));

        public async Task<int> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            string passwordHash = _passwordService.HashPassword(command.Password);

            // Create user entity
            var user = UserEntity.Create(
                command.Email,
                passwordHash,
                command.FirstName,
                command.LastName,
                command.Role);

            // Save user
            await _dbContext.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return user.Id;
        }
    }
}
