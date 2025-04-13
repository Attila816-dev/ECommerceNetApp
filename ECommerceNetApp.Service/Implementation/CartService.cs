using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries;
using FluentValidation;
using LiteDB;

namespace ECommerceNetApp.Service.Implementation
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemMapper _cartItemMapper;
        private readonly IValidator<CartItemDto> _cartItemValidator;

        public CartService(ICartRepository cartRepository, ICartItemMapper cartItemMapper, IValidator<CartItemDto> cartItemValidator)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _cartItemMapper = cartItemMapper ?? throw new ArgumentNullException(nameof(cartItemMapper));
            _cartItemValidator = cartItemValidator ?? throw new ArgumentNullException(nameof(cartItemValidator));
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

            return cart.Items.Select(_cartItemMapper.MapToDto).ToList();
        }

        public async Task AddItemToCartAsync(AddCartItemCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(command.CartId, nameof(command.CartId));
            ArgumentNullException.ThrowIfNull(command.Item);

            var validationResult = await _cartItemValidator.ValidateAsync(command.Item).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var cart = await _cartRepository.GetByIdAsync(command.CartId).ConfigureAwait(false);

            if (cart == null)
            {
                cart = new Cart(command.CartId);
            }

            // Use domain logic to add item
            cart.AddItem(_cartItemMapper.MapToEntity(command));

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
    }
}
