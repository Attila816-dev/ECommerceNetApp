using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerceNetApp.Service.Implementation.Validators.Product
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        private readonly ProductCatalogDbContext _dbContext;

        public UpdateProductCommandValidator(ProductCatalogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            RuleFor(x => x.Id)
                .MustAsync(ExistingProductIdAsync)
                .WithMessage("Product does not exist.");

            RuleFor(command => command.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(ProductEntity.MaxProductNameLength)
                .WithMessage($"Product name cannot exceed {ProductEntity.MaxProductNameLength} characters.");

            RuleFor(command => command.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Product price must be greater than or equal to zero.");

            RuleFor(command => command.Amount)
                .GreaterThan(0).WithMessage("Product amount must be greater than zero.");

            RuleFor(command => command.CategoryId)
                .GreaterThan(0).WithMessage("Product CategoryId must be a valid positive number.");
        }

        private async Task<bool> ExistingProductIdAsync(UpdateProductCommand command, int productId, CancellationToken cancellationToken)
        {
            return await _dbContext.Products.AnyAsync(c => c.Id == productId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
