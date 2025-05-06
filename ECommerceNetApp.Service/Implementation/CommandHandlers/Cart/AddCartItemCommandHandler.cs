using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class AddCartItemCommandHandler(ICartUnitOfWork cartUnitOfWork)
        : ICommandHandler<AddCartItemCommand>
    {
        private readonly ICartUnitOfWork _cartUnitOfWork = cartUnitOfWork;

        public async Task HandleAsync(AddCartItemCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var cart = await _cartUnitOfWork.CartRepository
                .GetByIdAsync(command.CartId, cancellationToken)
                .ConfigureAwait(false);

            cart ??= CartEntity.Create(command.CartId);

            // Use domain logic to add item
            cart.AddItem(
                command.Item.Id,
                command.Item.Name,
                Money.Create(command.Item.Price, command.Item.Currency),
                command.Item.Quantity,
                string.IsNullOrEmpty(command.Item.ImageUrl) ? null : ImageInfo.Create(command.Item.ImageUrl, command.Item.ImageAltText));

            await _cartUnitOfWork.CartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
            await _cartUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}