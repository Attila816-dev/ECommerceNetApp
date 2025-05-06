using ECommerceNetApp.Domain.Exceptions.Cart;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class RemoveCartItemCommandHandler(ICartUnitOfWork cartUnitOfWork)
        : IRequestHandler<RemoveCartItemCommand>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrEmpty(request.CartId, nameof(request.CartId));

            var cart = await _cartUnitOfWork.CartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                throw InvalidCartException.CartNotFound(request.CartId);
            }

            cart.RemoveItem(request.ItemId);
            await _cartUnitOfWork.CartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
            await _cartUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}