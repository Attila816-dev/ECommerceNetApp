using ECommerceNetApp.Domain;
using ECommerceNetApp.Persistence;

namespace ECommerceNetApp.Service
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        }

        public async Task<List<CartItemDto>?> GetCartItemsAsync(string cartId)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));
            }

            var cart = await _cartRepository.GetCartAsync(cartId).ConfigureAwait(false);

            if (cart == null)
            {
                return null;
            }

            return cart.Items.Select(MapToDto).ToList();
        }

        public async Task AddItemToCartAsync(string cartId, CartItemDto item)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));
            }

            ValidateCartItem(item);

            var cart = await GetOrCreateCartAsync(cartId).ConfigureAwait(false);

            // Check if item already exists
            var existingItem = cart.Items.FirstOrDefault(i => i.Id == item.Id);

            if (existingItem != null)
            {
                // Update existing item's quantity
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                // Add new item
                cart.Items.Add(MapToDomain(item));
            }

            await _cartRepository.SaveCartAsync(cart).ConfigureAwait(false);
        }

        public async Task RemoveItemFromCartAsync(string cartId, int itemId)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));
            }

            var cart = await _cartRepository.GetCartAsync(cartId).ConfigureAwait(false);

            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart with ID {cartId} not found.");
            }

            var itemToRemove = cart.Items.FirstOrDefault(i => i.Id == itemId);

            if (itemToRemove == null)
            {
                throw new KeyNotFoundException($"Item with ID {itemId} not found in cart.");
            }

            cart.Items.Remove(itemToRemove);
            await _cartRepository.SaveCartAsync(cart).ConfigureAwait(false);
        }

        public async Task UpdateItemQuantityAsync(string cartId, int itemId, int quantity)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
            }

            var cart = await _cartRepository.GetCartAsync(cartId).ConfigureAwait(false);

            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart with ID {cartId} not found.");
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                throw new KeyNotFoundException($"Item with ID {itemId} not found in cart.");
            }

            item.Quantity = quantity;
            await _cartRepository.SaveCartAsync(cart).ConfigureAwait(false);
        }

        public async Task<decimal> GetCartTotalAsync(string cartId)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));
            }

            var cart = await _cartRepository.GetCartAsync(cartId).ConfigureAwait(false);

            if (cart == null)
            {
                return 0; // Empty cart has zero total
            }

            return cart.Items.Aggregate(0M, (total, item) => total + (item.Price * item.Quantity));
        }

        private static CartItemDto MapToDto(CartItem item)
        {
            return new CartItemDto
            {
                Id = item.Id,
                Name = item.Name,
                ImageUrl = item.ImageUrl,
                ImageAltText = item.ImageAltText,
                Price = item.Price,
                Quantity = item.Quantity,
            };
        }

        private static CartItem MapToDomain(CartItemDto dto)
        {
            return new CartItem
            {
                Id = dto.Id,
                Name = dto.Name,
                ImageUrl = dto.ImageUrl,
                ImageAltText = dto.ImageAltText,
                Price = dto.Price,
                Quantity = dto.Quantity,
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
            var cart = await _cartRepository.GetCartAsync(cartId).ConfigureAwait(false);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = cartId,
                };
            }

            return cart;
        }
    }
}
