using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Cart;
using LiteDB;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartItemsQueryHandler(ICartRepository cartRepository)
        : IQueryHandler<GetCartItemsQuery, List<CartItemDto>?>
    {
        private readonly ICartRepository _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));

        public async Task<List<CartItemDto>?> HandleAsync(GetCartItemsQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(query.CartId);

            var cart = await _cartRepository.GetByIdAsync(query.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            return cart.Items.Select(CartItemDto.Create).ToList();
        }
    }
}
