using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.Commands.Product;
using FluentValidation;

namespace ECommerceNetApp.Service.Validators.Product
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(ProductEntity.MaxProductNameLength)
                .WithMessage($"Product name cannot exceed {ProductEntity.MaxProductNameLength} characters.");

            RuleFor(command => command.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Product price must be greater than or equal to zero.");

            RuleFor(command => command.Amount)
                .GreaterThan(0).WithMessage("Product amount must be greater than zero.");

            RuleFor(command => command.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be a valid positive number.");
        }
    }
}
