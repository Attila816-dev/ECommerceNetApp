using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using FluentValidation;

namespace ECommerceNetApp.Service.Validators.User
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork;

        public RegisterUserCommandValidator(IProductCatalogUnitOfWork productCatalogUnitOfWork)
        {
            _productCatalogUnitOfWork = productCatalogUnitOfWork ?? throw new ArgumentNullException(nameof(productCatalogUnitOfWork));

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .MustAsync(UserDoesNotExistWithEmailAsync)
                .WithMessage("Email already registered.");
        }

        private async Task<bool> UserDoesNotExistWithEmailAsync(RegisterUserCommand command, string email, CancellationToken cancellationToken)
        {
            var userExistsWithEmail = await _productCatalogUnitOfWork.UserRepository.GetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);
            return userExistsWithEmail == null;
        }
    }
}
