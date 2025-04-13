using ECommerceNetApp.Domain.Exceptions;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers
{
    public class UpdateCartItemQuantityCommandHandler(ICartRepository cartRepository)
        : IRequestHandler<UpdateCartItemQuantityCommand>
    {
        private readonly ICartRepository _cartRepository = cartRepository;

        public async Task Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrEmpty(request.CartId, nameof(request.CartId));

            if (request.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.");
            }

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
