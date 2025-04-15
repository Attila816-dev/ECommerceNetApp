using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Queries.Cart;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers
{
    public class GetCartTotalQueryHandler(ICartRepository cartRepository)
        : IRequestHandler<GetCartTotalQuery, decimal?>
    {
        private readonly ICartRepository _cartRepository = cartRepository;

        public async Task<decimal?> Handle(GetCartTotalQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrEmpty(request.CartId, nameof(request.CartId));

            var cart = await _cartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            var cartTotal = cart.CalculateTotal();
            return cartTotal.Amount;
        }
    }
}
