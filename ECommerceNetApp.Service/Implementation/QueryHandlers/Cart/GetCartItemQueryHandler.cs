using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Cart;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartItemQueryHandler(ICartUnitOfWork cartUnitOfWork)
        : IRequestHandler<GetCartItemQuery, CartItemDto?>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task<CartItemDto?> Handle(GetCartItemQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.CartId, nameof(request.CartId));

            var cartItem = await _cartUnitOfWork.CartRepository.GetCartItemAsync(request.CartId, request.ItemId, cancellationToken).ConfigureAwait(false);

            if (cartItem == null)
            {
                return null;
            }

            return CartItemDto.Create(cartItem);
        }
    }
}
