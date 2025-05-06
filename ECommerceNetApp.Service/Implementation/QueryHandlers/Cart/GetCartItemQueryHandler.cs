using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Cart;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartItemQueryHandler(ICartUnitOfWork cartUnitOfWork)
        : IQueryHandler<GetCartItemQuery, CartItemDto?>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task<CartItemDto?> HandleAsync(GetCartItemQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(query.CartId, nameof(query.CartId));

            var cartItem = await _cartUnitOfWork.CartRepository.GetCartItemAsync(query.CartId, query.ItemId, cancellationToken).ConfigureAwait(false);

            if (cartItem == null)
            {
                return null;
            }

            return CartItemDto.Create(cartItem);
        }
    }
}
