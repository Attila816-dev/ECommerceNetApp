using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class UpdateCartItemQuantityCommandHandler(
        ICartRepository cartRepository)
        : ICommandHandler<UpdateCartItemQuantityCommand>
    {
        private readonly ICartRepository _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));

        public async Task HandleAsync(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                throw InvalidCartException.CartNotFound(command.CartId);
            }

            cart.UpdateItemQuantity(command.ItemId, command.Quantity);
            await _cartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
        }
    }
}
