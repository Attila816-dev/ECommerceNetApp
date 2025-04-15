using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries;
using LiteDB;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers
{
    public class GetCartItemsQueryHandler(
        ICartRepository cartRepository,
        ICartItemMapper cartItemMapper)
        : IRequestHandler<GetCartItemsQuery, List<CartItemDto>?>
    {
        private readonly ICartRepository _cartRepository = cartRepository;
        private readonly ICartItemMapper _cartItemMapper = cartItemMapper;

        public async Task<List<CartItemDto>?> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.CartId, nameof(request.CartId));

            var cart = await _cartRepository.GetByIdAsync(request.CartId, cancellationToken).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            return cart.Items.Select(_cartItemMapper.MapToDto).ToList();
        }
    }
}
