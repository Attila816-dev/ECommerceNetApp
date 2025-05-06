using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Queries.Cart;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartTotalQueryHandler(ICartUnitOfWork cartUnitOfWork)
        : IRequestHandler<GetCartTotalQuery, decimal?>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task<decimal?> Handle(GetCartTotalQuery request, CancellationToken cancellationToken)
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
