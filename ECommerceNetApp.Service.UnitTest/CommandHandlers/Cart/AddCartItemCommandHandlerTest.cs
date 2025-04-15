using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Cart;
using ECommerceNetApp.Service.Implementation.Mappers;
using ECommerceNetApp.Service.Implementation.Validators.Cart;
using FluentValidation;
using Moq;
using Shouldly;
using CartEntity = ECommerceNetApp.Domain.Entities.Cart;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Cart
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
                    It.Is<CartEntity>(c => c.Items.Count == 1 && c.Items.First().Name == "New Item"),
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
                    It.Is<CartEntity>(c => c.Items.Count == 1 && c.Items.First().Quantity == 3),
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

        private static CartEntity CreateTestCart(string cartId)
            => new CartEntity(cartId);

        private void SetupMockRepository(CartEntity cart)
        {
            _mockRepository.Setup(r => r.GetByIdAsync(cart.Id, CancellationToken.None)).ReturnsAsync(cart);
        }
    }
}
