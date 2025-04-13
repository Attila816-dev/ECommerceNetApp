using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries;
using LiteDB;

namespace ECommerceNetApp.Service.Implementation
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        }

        public async Task<List<CartItemDto>?> GetCartItemsAsync(GetCartItemsQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);
            if (string.IsNullOrEmpty(query.CartId))
            {
                throw new ArgumentException("Cart ID cannot be empty.");
            }

            var cart = await _cartRepository.GetByIdAsync(query.CartId).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            return cart.Items.Select(MapToDto).ToList();
        }

        public async Task AddItemToCartAsync(AddCartItemCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(command.CartId, nameof(command.CartId));
            ArgumentNullException.ThrowIfNull(command.Item);
            ValidateCartItem(command.Item);

            var cart = await GetOrCreateCartAsync(command.CartId).ConfigureAwait(false);

            // Create domain entity from DTO
            var cartItem = new CartItem(
                command.Item.Id,
                command.Item.Name,
                new Money(command.Item.Price, command.Item.Currency),
                command.Item.Quantity,
                string.IsNullOrEmpty(command.Item.ImageUrl) ? null : new ImageInfo(command.Item.ImageUrl, command.Item.ImageAltText));

            // Use domain logic to add item
            cart.AddItem(cartItem);

            await _cartRepository.SaveAsync(cart).ConfigureAwait(false);
        }

        public async Task RemoveItemFromCartAsync(RemoveCartItemCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(command.CartId, nameof(command.CartId));

            var cart = await _cartRepository.GetByIdAsync(command.CartId).ConfigureAwait(false);

            if (cart == null)
            {
                throw new CartNotFoundException(command.CartId);
            }

            cart.RemoveItem(command.ItemId);
            await _cartRepository.SaveAsync(cart).ConfigureAwait(false);
        }

        public async Task UpdateItemQuantityAsync(UpdateCartItemQuantityCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(command.CartId, nameof(command.CartId));

            if (command.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.");
            }

            var cart = await _cartRepository.GetByIdAsync(command.CartId).ConfigureAwait(false);

            if (cart == null)
            {
                throw new CartNotFoundException(command.CartId);
            }

            cart.UpdateItemQuantity(command.ItemId, command.Quantity);
            await _cartRepository.SaveAsync(cart).ConfigureAwait(false);
        }

        public async Task<decimal> GetCartTotalAsync(GetCartTotalQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrEmpty(query.CartId, nameof(query.CartId));

            var cart = await _cartRepository.GetByIdAsync(query.CartId).ConfigureAwait(false);

            if (cart == null)
            {
                return 0; // Empty cart has zero total
            }

            var cartTotal = cart.CalculateTotal();
            return cartTotal.Amount;
        }

        private static CartItemDto MapToDto(CartItem item)
        {
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

        private static void ValidateCartItem(CartItemDto item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (item.Id <= 0)
            {
                throw new ArgumentException("Item ID must be a positive number.", nameof(item));
            }

            if (string.IsNullOrEmpty(item.Name))
            {
                throw new ArgumentException("Item name is required.", nameof(item));
            }

            if (item.Price <= 0)
            {
                throw new ArgumentException("Item price must be greater than zero.", nameof(item));
            }

            if (item.Quantity <= 0)
            {
                throw new ArgumentException("Item quantity must be greater than zero.", nameof(item));
            }
        }

        private async Task<Cart> GetOrCreateCartAsync(string cartId)
        {
            var cart = await _cartRepository.GetByIdAsync(cartId).ConfigureAwait(false);

            if (cart == null)
            {
                cart = new Cart(cartId);
            }

            return cart;
        }
    }
}
