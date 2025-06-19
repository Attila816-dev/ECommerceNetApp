using ECommerceNetApp.Service.Commands.Cart;
using FluentValidation;

namespace ECommerceNetApp.Service.Implementation.Validators.Cart
{
    public class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
    {
        public UpdateCartItemQuantityCommandValidator()
        {
            RuleFor(x => x.CartId)
                .NotEmpty()
                .WithMessage("Cart Id is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Cart item quantity must be greater than zero.");
        }
    }
}
