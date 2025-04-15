using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using FluentValidation;
using MediatR;
using CartEntity = ECommerceNetApp.Domain.Entities.Cart;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class AddCartItemCommandHandler(
        ICartRepository cartRepository,
        ICartItemMapper cartItemMapper,
        IValidator<CartItemDto> cartItemValidator)
        : IRequestHandler<AddCartItemCommand>
    {
        private readonly ICartRepository _cartRepository = cartRepository;
        private readonly ICartItemMapper _cartItemMapper = cartItemMapper;
        private readonly IValidator<CartItemDto> _cartItemValidator = cartItemValidator;

        public async Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrEmpty(request.CartId, nameof(request.CartId));

            var validationResult = await _cartItemValidator.ValidateAsync(request.Item, cancellationToken).ConfigureAwait(false);
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