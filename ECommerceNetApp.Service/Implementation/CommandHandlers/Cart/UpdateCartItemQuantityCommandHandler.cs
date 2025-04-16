using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Cart;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class UpdateCartItemQuantityCommandHandler(ICartRepository cartRepository)
        : IRequestHandler<UpdateCartItemQuantityCommand>
    {
        private readonly ICartRepository _cartRepository = cartRepository;

        public async Task Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrEmpty(request.CartId, nameof(request.CartId));

            var cart = await _cartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                throw new CartNotFoundException(request.CartId);
            }

            cart.UpdateItemQuantity(request.ItemId, request.Quantity);
            await _cartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
        }
    }
}
