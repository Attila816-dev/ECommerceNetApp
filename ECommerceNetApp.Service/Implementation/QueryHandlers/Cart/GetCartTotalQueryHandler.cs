using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Queries.Cart;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartTotalQueryHandler(ICartRepository cartRepository)
        : IQueryHandler<GetCartTotalQuery, decimal?>
    {
        private readonly ICartRepository _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));

        public async Task<decimal?> HandleAsync(GetCartTotalQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrEmpty(query.CartId);

            var cart = await _cartRepository.GetByIdAsync(query.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            var cartTotal = cart.CalculateTotal();
            return cartTotal.Amount;
        }
    }
}
