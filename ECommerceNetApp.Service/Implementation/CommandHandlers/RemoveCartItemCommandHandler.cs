using ECommerceNetApp.Domain.Exceptions;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Cart;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers
{
    public class RemoveCartItemCommandHandler(ICartRepository cartRepository)
        : IRequestHandler<RemoveCartItemCommand>
    {
        private readonly ICartRepository _cartRepository = cartRepository;

        public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrEmpty(request.CartId, nameof(request.CartId));

            var cart = await _cartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                throw new CartNotFoundException(request.CartId);
            }

            cart.RemoveItem(request.ItemId);
            await _cartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
        }
    }
}