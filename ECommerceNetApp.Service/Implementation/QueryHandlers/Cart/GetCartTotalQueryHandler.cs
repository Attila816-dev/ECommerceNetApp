using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Queries.Cart;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartTotalQueryHandler(ICartUnitOfWork cartUnitOfWork)
        : IQueryHandler<GetCartTotalQuery, decimal?>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task<decimal?> HandleAsync(GetCartTotalQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrEmpty(request.CartId, nameof(request.CartId));

            var cart = await _cartUnitOfWork.CartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            var cartTotal = cart.CalculateTotal();
            return cartTotal.Amount;
        }
    }
}
