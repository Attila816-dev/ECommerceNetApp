using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Cart;
using LiteDB;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Cart
{
    public class GetCartItemsQueryHandler(
        ICartUnitOfWork cartUnitOfWork)
        : IRequestHandler<GetCartItemsQuery, List<CartItemDto>?>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task<List<CartItemDto>?> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.CartId, nameof(request.CartId));

            var cart = await _cartUnitOfWork.CartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            return cart.Items.Select(MapToDto).ToList();
        }

        private static CartItemDto MapToDto(CartItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            return new CartItemDto
            {
                Id = item.Id,
                Name = item.Name,
                ImageUrl = item.Image?.Url,
                ImageAltText = item.Image?.AltText,
                Price = item.Price?.Amount ?? 0,
                Currency = item.Price?.Currency,
                Quantity = item.Quantity,
            };
        }
    }
}
