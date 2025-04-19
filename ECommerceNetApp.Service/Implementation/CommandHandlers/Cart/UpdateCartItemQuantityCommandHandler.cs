using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class UpdateCartItemQuantityCommandHandler(
        ICartUnitOfWork cartUnitOfWork,
        IValidator<UpdateCartItemQuantityCommand> validator)
        : IRequestHandler<UpdateCartItemQuantityCommand>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;
        private readonly IValidator<UpdateCartItemQuantityCommand> _validator = validator;

        public async Task Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var cart = await _cartUnitOfWork.CartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                throw new CartNotFoundException(request.CartId);
            }

            cart.UpdateItemQuantity(request.ItemId, request.Quantity);
            await _cartUnitOfWork.CartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
            await _cartUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
