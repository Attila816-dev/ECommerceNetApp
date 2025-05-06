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

        public async Task<List<CartItemDto>?> HandleAsync(GetCartItemsQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(query.CartId, nameof(query.CartId));

            var cart = await _cartUnitOfWork.CartRepository.GetByIdAsync(query.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            return cart.Items.Select(CartItemDto.Create).ToList();
        }
    }
}
