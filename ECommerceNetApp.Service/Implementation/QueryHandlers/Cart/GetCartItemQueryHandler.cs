using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Cart;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartItemQueryHandler(ICartRepository cartRepository)
        : IQueryHandler<GetCartItemQuery, CartItemDto?>
    {
        private readonly ICartRepository _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));

        public async Task<CartItemDto?> HandleAsync(GetCartItemQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(query.CartId, nameof(query.CartId));

            var cartItem = await _cartRepository.GetCartItemAsync(query.CartId, query.ItemId, cancellationToken).ConfigureAwait(false);

            if (cartItem == null)
            {
                return null;
            }

            return CartItemDto.Create(cartItem);
        }
    }
}
