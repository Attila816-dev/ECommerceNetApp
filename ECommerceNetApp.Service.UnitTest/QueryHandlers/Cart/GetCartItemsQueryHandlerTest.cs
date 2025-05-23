﻿using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Cart;
using ECommerceNetApp.Service.Queries.Cart;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Cart
{
    public class GetCartItemsQueryHandlerTest
    {
        private readonly GetCartItemsQueryHandler _queryHandler;
        private readonly Mock<ICartRepository> _mockRepository;

        public GetCartItemsQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICartRepository>();
            _queryHandler = new GetCartItemsQueryHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task GetCartItems_EmptyCart_ReturnsNull()
        {
            // Arrange
            string testCartId = "test-cart-123";
            _mockRepository
                .Setup(r => r.GetByIdAsync(It.Is<string>(c => c == testCartId), CancellationToken.None))
                .ReturnsAsync((CartEntity?)null);

            // Act
            var result = await _queryHandler.HandleAsync(new GetCartItemsQuery(testCartId), CancellationToken.None);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetCartItems_WithItems_ReturnsItems()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = CartEntity.Create(testCartId);

            cart.AddItem(1, "Test Item", Money.From(10.99m), 2);

            _mockRepository
                .Setup(r => r.GetByIdAsync(testCartId, CancellationToken.None))
                .ReturnsAsync(cart);

            // Act
            var result = await _queryHandler.HandleAsync(new GetCartItemsQuery(testCartId), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result.ShouldContain(c => c.Name == "Test Item" && c.Quantity == 2);
        }
    }
}
