using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Service.Commands;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;

namespace ECommerceNetApp.Service.Mapping
{
    public class CartItemMapper : ICartItemMapper
    {
        public CartItemDto MapToDto(CartItem item)
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

        public CartItem MapToEntity(AddCartItemCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(command.Item);

            return new CartItem(
                command.Item.Id,
                command.Item.Name,
                new Money(command.Item.Price, command.Item.Currency),
                command.Item.Quantity,
                string.IsNullOrEmpty(command.Item.ImageUrl) ? null : new ImageInfo(command.Item.ImageUrl, command.Item.ImageAltText));
        }
    }
}
