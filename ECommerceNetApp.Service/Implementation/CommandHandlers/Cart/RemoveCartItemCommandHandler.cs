using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class RemoveCartItemCommandHandler(ICartRepository cartRepository)
        : ICommandHandler<RemoveCartItemCommand>
    {
        private readonly ICartRepository _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));

        public async Task HandleAsync(RemoveCartItemCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(command.CartId, nameof(command.CartId));

            var cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                throw InvalidCartException.CartNotFound(command.CartId);
            }

            cart.RemoveItem(command.ItemId);
            await _cartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
        }
    }
}