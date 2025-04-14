using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers;
using ECommerceNetApp.Service.Implementation.Mappers;
using ECommerceNetApp.Service.Implementation.Validators;
using FluentValidation;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers
{
    public class AddCartItemCommandHandlerTest
    {
        private readonly AddCartItemCommandHandler _commandHandler;
        private readonly Mock<ICartRepository> _mockRepository;
        private readonly CartItemMapper _cartItemMapper;
        private readonly CartItemValidator _cartItemValidator;

        public AddCartItemCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICartRepository>();
            _cartItemMapper = new CartItemMapper();
            _cartItemValidator = new CartItemValidator();
            _commandHandler = new AddCartItemCommandHandler(_mockRepository.Object, _cartItemMapper, _cartItemValidator);
        }

        [Fact]
        public async Task CallAddCartItem_AddsItemToCart()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = CreateTestCart(testCartId);
            SetupMockRepository(cart);

            var itemDto = new CartItemDto
            {
                Id = 1,
                Name = "New Item",
                Price = 15.99m,
                Quantity = 1,
            };

            // Act
            await _commandHandler.Handle(new AddCartItemCommand(testCartId, itemDto), CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(
                    It.Is<Cart>(c => c.Items.Count == 1 && c.Items.First().Name == "New Item"),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task CallAddCartItemWithoutName_ThrowsValidationException()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = CreateTestCart(testCartId);
            SetupMockRepository(cart);

            var itemDto = new CartItemDto
            {
                Id = 1,
                Price = 15.99m,
                Quantity = 1,
            };

            // Act & Assert
            var validationException = await Should.ThrowAsync<ValidationException>(async () =>
            {
                await _commandHandler.Handle(new AddCartItemCommand(testCartId, itemDto), CancellationToken.None);
            });

            validationException.Message.ShouldContain("Item name is required.");
        }

        [Fact]
        public async Task AddItemToCart_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = CreateTestCart(testCartId);

            cart.AddItem(new CartItem(1, "Existing Item", new Money(15.99m), 1));
            SetupMockRepository(cart);

            var itemDto = new CartItemDto
            {
                Id = 1,
                Name = "Existing Item",
                Price = 15.99m,
                Quantity = 2,
            };

            // Act
            await _commandHandler.Handle(new AddCartItemCommand(testCartId, itemDto), CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(
                    It.Is<Cart>(c => c.Items.Count == 1 && c.Items.First().Quantity == 3),
                    CancellationToken.None),
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
            var validationException = await Should.ThrowAsync<ValidationException>(
                () => _commandHandler.Handle(new AddCartItemCommand(testCartId, itemDto), CancellationToken.None));

            validationException.Message.ShouldContain("Item ID must be a positive number.");
        }

        [Fact]
        public async Task AddItemToCart_WithNegativeQuantity_ThrowsException()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var itemDto = new CartItemDto
            {
                Id = 2, // Invalid ID
                Name = "Test Item",
                Price = 10.99m,
                Quantity = -1,
            };

            // Act & Assert
            var validationException = await Should.ThrowAsync<ValidationException>(
                () => _commandHandler.Handle(new AddCartItemCommand(testCartId, itemDto), CancellationToken.None));

            validationException.Message.ShouldContain("Item quantity must be greater than zero.");
        }

        [Fact]
        public async Task AddItemToCart_WithNegativePrice_ThrowsException()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var itemDto = new CartItemDto
            {
                Id = 2, // Invalid ID
                Name = "Test Item",
                Price = -10.99m,
                Quantity = 1,
            };

            // Act & Assert
            var validationException = await Should.ThrowAsync<ValidationException>(
                () => _commandHandler.Handle(new AddCartItemCommand(testCartId, itemDto), CancellationToken.None));

            validationException.Message.ShouldContain("Item price must be greater than zero.");
        }

        private static Cart CreateTestCart(string cartId)
            => new Cart(cartId);

        private void SetupMockRepository(Cart cart)
        {
            _mockRepository.Setup(r => r.GetByIdAsync(cart.Id, CancellationToken.None)).ReturnsAsync(cart);
        }
    }
}

/*
 using ECommerceNetApp.Domain;
using ECommerceNetApp.Persistence.Interfaces;
using Moq;
using Shouldly;

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
            result.ShouldBeNull();
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
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result.First().Name.ShouldBe("Test Item");
            result.First().Quantity.ShouldBe(2);
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
            await Should.ThrowAsync<ArgumentException>(
                () => _cartService.AddItemToCartAsync(testCartId, itemDto));
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
            cartTotal.ShouldBe(52.97m); // 10.99 + (20.99 * 2)
        }
    }
}
 */