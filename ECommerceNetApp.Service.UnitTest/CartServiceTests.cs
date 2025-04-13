using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation;
using ECommerceNetApp.Service.Queries;
using FluentAssertions;
using Moq;

namespace ECommerceNetApp.Service.UnitTest
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _mockRepository;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _mockRepository = new Mock<ICartRepository>();
            _cartService = new CartService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetCartItems_EmptyCart_ReturnsNull()
        {
            // Arrange
            string testCartId = "test-cart-123";
            _mockRepository.Setup(r => r.GetByIdAsync(It.Is<string>(c => c == testCartId))).ReturnsAsync((Cart?)null);

            // Act
            var result = await _cartService.GetCartItemsAsync(new GetCartItemsQuery(testCartId));

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCartItems_WithItems_ReturnsItems()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = new Cart(testCartId);

            cart.AddItem(new CartItem(1, "Test Item", new Money(10.99m), 2));

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId)).ReturnsAsync(cart);

            // Act
            var result = await _cartService.GetCartItemsAsync(new GetCartItemsQuery(testCartId));

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Test Item");
            result.First().Quantity.Should().Be(2);
        }

        [Fact]
        public async Task AddItemToCart_NewItem_AddsToCart()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = new Cart(testCartId);

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId))
                .ReturnsAsync(cart);

            var itemDto = new CartItemDto
            {
                Id = 1,
                Name = "New Item",
                Price = 15.99m,
                Quantity = 1,
                Currency = "EUR",
            };

            // Act
            await _cartService.AddItemToCartAsync(new AddCartItemCommand(testCartId, itemDto));

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(It.Is<Cart>(c =>
                    c.Items.Count == 1 &&
                    c.Items.First().Name == "New Item")),
                Times.Once);
        }

        [Fact]
        public async Task AddItemToCart_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = new Cart(testCartId);

            cart.AddItem(new CartItem(1, "Existing Item", new Money(15.99m), 1));

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId))
                .ReturnsAsync(cart);

            var itemDto = new CartItemDto
            {
                Id = 1,
                Name = "Existing Item",
                Price = 15.99m,
                Quantity = 2,
                Currency = "EUR",
            };

            // Act
            await _cartService.AddItemToCartAsync(new AddCartItemCommand(testCartId, itemDto));

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(It.Is<Cart>(c =>
                    c.Items.Count == 1 &&
                    c.Items.First().Quantity == 3)),
                Times.Once);
        }

        [Fact]
        public async Task RemoveItemFromCart_ExistingItem_RemovesFromCart()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = new Cart(testCartId);

            cart.AddItem(new CartItem(1, "Item 1", new Money(10.99m), 1));
            cart.AddItem(new CartItem(2, "Item 2", new Money(20.99m), 2));

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId))
                .ReturnsAsync(cart);

            // Act
            await _cartService.RemoveItemFromCartAsync(new RemoveCartItemCommand(testCartId, 1));

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(It.Is<Cart>(c =>
                    c.Items.Count == 1 &&
                    c.Items.First().Id == 2)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateItemQuantity_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = new Cart(testCartId);

            cart.AddItem(new CartItem(1, "Test Item", new Money(10.99m), 1));

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId))
                .ReturnsAsync(cart);

            // Act
            await _cartService.UpdateItemQuantityAsync(new UpdateCartItemQuantityCommand(testCartId, 1, 5));

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(It.Is<Cart>(c =>
                    c.Items.Count == 1 &&
                    c.Items.First().Quantity == 5)),
                Times.Once);
        }

        [Fact]
        public async Task AddItemToCart_InvalidItem_ThrowsException()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var itemDto = new CartItemDto
            {
                Id = 0, // Invalid ID
                Name = "Test Item",
                Price = 10.99m,
                Quantity = 1,
                Currency = "EUR",
            };

            // Act & Assert
            await _cartService.Invoking(s => s.AddItemToCartAsync(new AddCartItemCommand(testCartId, itemDto)))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetCartTotal_ReturnsExpectedValue()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = new Cart(testCartId);

            cart.AddItem(new CartItem(1, "Item 1", new Money(10.99m), 1));
            cart.AddItem(new CartItem(2, "Item 2", new Money(20.99m), 2));

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId))
                .ReturnsAsync(cart);

            // Act
            var cartTotal = await _cartService.GetCartTotalAsync(new GetCartTotalQuery(testCartId));

            // Assert
            cartTotal.Should().Be(52.97m); // 10.99 + (20.99 * 2)
        }
    }
}