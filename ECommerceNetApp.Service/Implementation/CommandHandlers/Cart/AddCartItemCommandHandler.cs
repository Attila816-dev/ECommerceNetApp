using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.Interfaces.Mappers.Cart;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class AddCartItemCommandHandler(
        ICartUnitOfWork cartUnitOfWork,
        ICartItemMapper cartItemMapper)
        : IRequestHandler<AddCartItemCommand>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;
        private readonly ICartItemMapper _cartItemMapper = cartItemMapper;

        public async Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var cart = await _cartUnitOfWork.CartRepository
                .GetByIdAsync(request.CartId, cancellationToken)
                .ConfigureAwait(false);

            if (cart == null)
            {
                cart = new CartEntity(request.CartId);
            }

            // Use domain logic to add item
            var cartItem = _cartItemMapper.MapToEntity(request);
            cart.AddItem(cartItem);

            await _cartUnitOfWork.CartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
            await _cartUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}