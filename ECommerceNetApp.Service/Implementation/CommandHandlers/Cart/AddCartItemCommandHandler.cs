using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.Interfaces.Mappers.Cart;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class AddCartItemCommandHandler(
        ICartRepository cartRepository,
        ICartItemMapper cartItemMapper,
        IValidator<AddCartItemCommand> validator)
        : IRequestHandler<AddCartItemCommand>
    {
        private readonly ICartRepository _cartRepository = cartRepository;
        private readonly ICartItemMapper _cartItemMapper = cartItemMapper;
        private readonly IValidator<AddCartItemCommand> _validator = validator;

        public async Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var cart = await _cartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                cart = new CartEntity(request.CartId);
            }

            // Use domain logic to add item
            cart.AddItem(_cartItemMapper.MapToEntity(request));

            await _cartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
        }
    }
}