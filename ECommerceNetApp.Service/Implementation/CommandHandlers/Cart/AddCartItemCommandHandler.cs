using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class AddCartItemCommandHandler(ICartUnitOfWork cartUnitOfWork)
        : IRequestHandler<AddCartItemCommand>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var cart = await _cartUnitOfWork.CartRepository
                .GetByIdAsync(request.CartId, cancellationToken)
                .ConfigureAwait(false);

            cart ??= CartEntity.Create(request.CartId);

            // Use domain logic to add item
            cart.AddItem(
                request.Item.Id,
                request.Item.Name,
                Money.Create(request.Item.Price, request.Item.Currency),
                request.Item.Quantity,
                string.IsNullOrEmpty(request.Item.ImageUrl) ? null : ImageInfo.Create(request.Item.ImageUrl, request.Item.ImageAltText));

            await _cartUnitOfWork.CartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
            await _cartUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}