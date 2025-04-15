using ECommerceNetApp.Service.DTO;
using FluentValidation;

namespace ECommerceNetApp.Service.Implementation.Validators.Cart
{
    public class CartItemValidator : AbstractValidator<CartItemDto>
    {
        public CartItemValidator()
        {
            RuleFor(item => item)
                .NotNull()
                .WithMessage("Cart item cannot be null.");

            RuleFor(item => item.Id)
                .GreaterThan(0)
                .WithMessage("Item ID must be a positive number.");

            RuleFor(item => item.Name)
                .NotEmpty()
                .WithMessage("Item name is required.");

            RuleFor(item => item.Price)
                .GreaterThan(0)
                .WithMessage("Item price must be greater than zero.");

            RuleFor(item => item.Quantity)
                .GreaterThan(0)
                .WithMessage("Item quantity must be greater than zero.");
        }
    }
}
