using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Cart;
using LiteDB;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartQueryHandler(ICartRepository cartRepository)
        : IQueryHandler<GetCartQuery, CartDto?>
    {
        private readonly ICartRepository _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));

        public async Task<CartDto?> HandleAsync(GetCartQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(query.CartId);

            var cart = await _cartRepository.GetByIdAsync(query.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            var cartDto = new CartDto(
                cart.Id,
                cart.Items.Select(CartItemDto.Create).ToList());
            return cartDto;
        }
    }
}
