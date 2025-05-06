using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Cart;
using LiteDB;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartItemsQueryHandler(ICartUnitOfWork cartUnitOfWork)
        : IQueryHandler<GetCartItemsQuery, List<CartItemDto>?>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task<List<CartItemDto>?> HandleAsync(GetCartItemsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.CartId, nameof(request.CartId));

            var cart = await _cartUnitOfWork.CartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            return cart.Items.Select(CartItemDto.Create).ToList();
        }
    }
}
