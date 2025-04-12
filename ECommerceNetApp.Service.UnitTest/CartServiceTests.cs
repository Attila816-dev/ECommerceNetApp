using ECommerceNetApp.Domain;
using ECommerceNetApp.Persistence;
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
            _mockRepository.Setup(r => r.GetCartAsync(It.Is<string>(c => c == testCartId))).ReturnsAsync((Cart?)null);

            // Act
            var result = await _cartService.GetCartItemsAsync(testCartId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCartItems_WithItems_ReturnsItems()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = new Cart
            {
                Id = testCartId,
            };

            cart.Items.Add(new CartItem { Id = 1, Name = "Test Item", Price = 10.99m, Quantity = 2 });

            _mockRepository.Setup(r => r.GetCartAsync(testCartId))
                .ReturnsAsync(cart);

            // Act
            var result = await _cartService.GetCartItemsAsync(testCartId);

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

            var cart = new Cart
            {
                Id = testCartId,
            };

            _mockRepository.Setup(r => r.GetCartAsync(testCartId))
                .ReturnsAsync(cart);

            var itemDto = new CartItemDto
            {
                Id = 1,
                Name = "New Item",
                Price = 15.99m,
                Quantity = 1,
            };

            // Act
            await _cartService.AddItemToCartAsync(testCartId, itemDto);

            // Assert
            _mockRepository.Verify(
                r => r.SaveCartAsync(It.Is<Cart>(c =>
                    c.Items.Count == 1 &&
                    c.Items[0].Name == "New Item")),
                Times.Once);
        }

        [Fact]
        public async Task AddItemToCart_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = new Cart
            {
                Id = testCartId,
            };

            cart.Items.Add(new CartItem { Id = 1, Name = "Existing Item", Price = 15.99m, Quantity = 1 });

            _mockRepository.Setup(r => r.GetCartAsync(testCartId))
                .ReturnsAsync(cart);

            var itemDto = new CartItemDto
            {
                Id = 1,
                Name = "Existing Item",
                Price = 15.99m,
                Quantity = 2,
            };

            // Act
            await _cartService.AddItemToCartAsync(testCartId, itemDto);

            // Assert
            _mockRepository.Verify(
                r => r.SaveCartAsync(It.Is<Cart>(c =>
                    c.Items.Count == 1 &&
                    c.Items[0].Quantity == 3)),
                Times.Once);
        }

        [Fact]
        public async Task RemoveItemFromCart_ExistingItem_RemovesFromCart()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = new Cart
            {
                Id = testCartId,
            };

            cart.Items.Add(new CartItem { Id = 1, Name = "Item 1", Price = 10.99m, Quantity = 1 });
            cart.Items.Add(new CartItem { Id = 2, Name = "Item 2", Price = 20.99m, Quantity = 2 });

            _mockRepository.Setup(r => r.GetCartAsync(testCartId))
                .ReturnsAsync(cart);

            // Act
            await _cartService.RemoveItemFromCartAsync(testCartId, 1);

            // Assert
            _mockRepository.Verify(
                r => r.SaveCartAsync(It.Is<Cart>(c =>
                    c.Items.Count == 1 &&
                    c.Items[0].Id == 2)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateItemQuantity_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = new Cart
            {
                Id = testCartId,
            };

            cart.Items.Add(new CartItem { Id = 1, Name = "Test Item", Price = 10.99m, Quantity = 1 });

            _mockRepository.Setup(r => r.GetCartAsync(testCartId))
                .ReturnsAsync(cart);

            // Act
            await _cartService.UpdateItemQuantityAsync(testCartId, 1, 5);

            // Assert
            _mockRepository.Verify(
                r => r.SaveCartAsync(It.Is<Cart>(c =>
                    c.Items.Count == 1 &&
                    c.Items[0].Quantity == 5)),
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
            };

            // Act & Assert
            await _cartService.Invoking(s => s.AddItemToCartAsync(testCartId, itemDto))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetCartTotal_ReturnsExpectedValue()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = new Cart
            {
                Id = testCartId,
            };

            cart.Items.Add(new CartItem { Id = 1, Name = "Item 1", Price = 10.99m, Quantity = 1 });
            cart.Items.Add(new CartItem { Id = 2, Name = "Item 2", Price = 20.99m, Quantity = 2 });

            _mockRepository.Setup(r => r.GetCartAsync(testCartId))
                .ReturnsAsync(cart);

            // Act
            var cartTotal = await _cartService.GetCartTotalAsync(testCartId);

            // Assert
            cartTotal.Should().Be(52.97m); // 10.99 + (20.99 * 2)
        }
    }
}