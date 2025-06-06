﻿using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Cart;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Cart
{
    public class AddCartItemCommandHandlerTest
    {
        private readonly AddCartItemCommandHandler _commandHandler;
        private readonly Mock<ICartRepository> _mockRepository;

        public AddCartItemCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICartRepository>();
            _commandHandler = new AddCartItemCommandHandler(_mockRepository.Object);
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
            await _commandHandler.HandleAsync(new AddCartItemCommand(testCartId, itemDto), CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(
                    It.Is<CartEntity>(c => c.Items.Count == 1 && c.Items.First().Name == "New Item"),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task AddItemToCart_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = CreateTestCart(testCartId);

            cart.AddItem(1, "Existing Item", Money.From(15.99m), 1);
            SetupMockRepository(cart);

            var itemDto = new CartItemDto
            {
                Id = 1,
                Name = "Existing Item",
                Price = 15.99m,
                Quantity = 2,
            };

            // Act
            await _commandHandler.HandleAsync(new AddCartItemCommand(testCartId, itemDto), CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(
                    It.Is<CartEntity>(c => c.Items.Count == 1 && c.Items.First().Quantity == 3),
                    CancellationToken.None),
                Times.Once);
        }

        private static CartEntity CreateTestCart(string cartId)
            => CartEntity.Create(cartId);

        private void SetupMockRepository(CartEntity cart)
        {
            _mockRepository.Setup(r => r.GetByIdAsync(cart.Id, CancellationToken.None)).ReturnsAsync(cart);
        }
    }
}