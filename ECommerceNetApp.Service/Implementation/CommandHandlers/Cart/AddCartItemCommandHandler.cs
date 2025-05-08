using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Cart
{
    public class AddCartItemCommandHandler(ICartRepository cartRepository)
        : ICommandHandler<AddCartItemCommand>
    {
        private readonly ICartRepository _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));

        public async Task HandleAsync(AddCartItemCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);

            var cart = (await _cartRepository.GetByIdAsync(command.CartId, cancellationToken).ConfigureAwait(false))
                ?? CartEntity.Create(command.CartId);

            // Use domain logic to add item
            cart.AddItem(
                command.Item.Id,
                command.Item.Name,
                Money.Create(command.Item.Price, command.Item.Currency),
                command.Item.Quantity,
                string.IsNullOrEmpty(command.Item.ImageUrl) ? null : ImageInfo.Create(command.Item.ImageUrl, command.Item.ImageAltText));

            await _cartRepository.SaveAsync(cart, cancellationToken).ConfigureAwait(false);
        }
    }
}