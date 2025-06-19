using ECommerceNetApp.Service.Commands.Cart;
using FluentValidation;

namespace ECommerceNetApp.Service.Implementation.Validators.Cart
{
    public class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
    {
        public AddCartItemCommandValidator()
        {
            RuleFor(x => x.CartId)
                .NotEmpty()
                .WithMessage("Cart Id is required.");

            RuleFor(x => x.Item)
                .NotNull()
                .WithMessage("Cart item cannot be null.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Item.Id)
                        .GreaterThan(0)
                        .WithMessage("Cart item Id must be a positive number.");

                    RuleFor(x => x.Item.Name)
                        .NotEmpty()
                        .WithMessage("Cart item name is required.");

                    RuleFor(x => x.Item.Price)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Cart item price must be greater than or equal to zero.");

                    RuleFor(x => x.Item.Quantity)
                        .GreaterThan(0)
                        .WithMessage("Cart item quantity must be greater than zero.");
                });
        }
    }
}
