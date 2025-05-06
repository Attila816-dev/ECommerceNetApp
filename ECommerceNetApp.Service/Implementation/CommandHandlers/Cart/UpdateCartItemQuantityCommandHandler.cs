using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class UpdateCartItemQuantityCommandHandler(
        ICartUnitOfWork cartUnitOfWork)
        : ICommandHandler<UpdateCartItemQuantityCommand>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task HandleAsync(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var cart = await _cartUnitOfWork.CartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                throw InvalidCartException.CartNotFound(request.CartId);
            }

            cart.UpdateItemQuantity(request.ItemId, request.Quantity);
            await _cartUnitOfWork.CartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
            await _cartUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
