using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Validators.User
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private readonly ProductCatalogDbContext _dbContext;

        public RegisterUserCommandValidator(ProductCatalogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .MustAsync(UserDoesNotExistWithEmailAsync)
                .WithMessage("Email already registered.");
        }

        private async Task<bool> UserDoesNotExistWithEmailAsync(RegisterUserCommand command, string email, CancellationToken cancellationToken)
        {
            var userExistsWithEmail = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToUpper() == email.ToUpper(), cancellationToken).ConfigureAwait(false);
            return userExistsWithEmail == null;
        }
    }
}
