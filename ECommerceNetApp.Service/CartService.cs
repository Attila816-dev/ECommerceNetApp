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

        public async Task<List<CartItemDto>> GetCartItemsAsync(string cartId)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));
            }

            var cart = await _cartRepository.GetCartAsync(cartId);

            if (cart == null)
            {
                // Return empty list for non-existent cart
                return new List<CartItemDto>();
            }

            return cart.Items.Select(MapToDto).ToList();
        }

        public async Task AddItemToCartAsync(string cartId, CartItemDto itemDto)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));
            }

            ValidateCartItem(itemDto);

            var cart = await GetOrCreateCartAsync(cartId);

            // Check if item already exists
            var existingItem = cart.Items.FirstOrDefault(i => i.Id == itemDto.Id);

            if (existingItem != null)
            {
                // Update existing item's quantity
                existingItem.Quantity += itemDto.Quantity;
            }
            else
            {
                // Add new item
                cart.Items.Add(MapToDomain(itemDto));
            }

            await _cartRepository.SaveCartAsync(cart);
        }

        public async Task RemoveItemFromCartAsync(string cartId, int itemId)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));
            }

            var cart = await _cartRepository.GetCartAsync(cartId);

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
            await _cartRepository.SaveCartAsync(cart);
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

            var cart = await _cartRepository.GetCartAsync(cartId);

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
            await _cartRepository.SaveCartAsync(cart);
        }

        private async Task<Cart> GetOrCreateCartAsync(string cartId)
        {
            var cart = await _cartRepository.GetCartAsync(cartId);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = cartId,
                    Items = new List<CartItem>()
                };
            }

            return cart;
        }

        public async Task<decimal> GetCartTotalAsync(string cartId)
        {
            if (string.IsNullOrEmpty(cartId))
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));

            var cart = await _cartRepository.GetCartAsync(cartId);

            if (cart == null)
                return 0; // Empty cart has zero total

            return cart.Items.Aggregate((decimal)0, (total, item) => total + (item.Price * item.Quantity));
        }

        private CartItemDto MapToDto(CartItem item)
        {
            return new CartItemDto
            {
                Id = item.Id,
                Name = item.Name,
                ImageUrl = item.ImageUrl,
                ImageAltText = item.ImageAltText,
                Price = item.Price,
                Quantity = item.Quantity
            };
        }

        private CartItem MapToDomain(CartItemDto dto)
        {
            return new CartItem
            {
                Id = dto.Id,
                Name = dto.Name,
                ImageUrl = dto.ImageUrl,
                ImageAltText = dto.ImageAltText,
                Price = dto.Price,
                Quantity = dto.Quantity
            };
        }

        private void ValidateCartItem(CartItemDto item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

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
    }
}
