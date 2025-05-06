using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Cart;
using LiteDB;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartQueryHandler(ICartUnitOfWork cartUnitOfWork)
        : IQueryHandler<GetCartQuery, CartDto?>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task<CartDto?> HandleAsync(GetCartQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.CartId, nameof(request.CartId));

            var cart = await _cartUnitOfWork.CartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

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
