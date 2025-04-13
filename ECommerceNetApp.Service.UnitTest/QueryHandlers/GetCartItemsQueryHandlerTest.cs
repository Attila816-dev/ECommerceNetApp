﻿using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Implementation.Mappers;
using ECommerceNetApp.Service.Implementation.QueryHandlers;
using ECommerceNetApp.Service.Queries;
using FluentAssertions;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers
{
    public class GetCartItemsQueryHandlerTest
    {
        private readonly GetCartItemsQueryHandler _queryHandler;
        private readonly Mock<ICartRepository> _mockRepository;
        private readonly CartItemMapper _cartItemMapper;

        public GetCartItemsQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICartRepository>();
            _cartItemMapper = new CartItemMapper();
            _queryHandler = new GetCartItemsQueryHandler(_mockRepository.Object, _cartItemMapper);
        }

        [Fact]
        public async Task GetCartItems_EmptyCart_ReturnsNull()
        {
            // Arrange
            string testCartId = "test-cart-123";
            _mockRepository
                .Setup(r => r.GetByIdAsync(It.Is<string>(c => c == testCartId), CancellationToken.None))
                .ReturnsAsync((Cart?)null);

            // Act
            var result = await _queryHandler.Handle(new GetCartItemsQuery(testCartId), CancellationToken.None);

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

            _mockRepository
                .Setup(r => r.GetByIdAsync(testCartId, CancellationToken.None))
                .ReturnsAsync(cart);

            // Act
            var result = await _queryHandler.Handle(new GetCartItemsQuery(testCartId), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Test Item");
            result.First().Quantity.Should().Be(2);
        }
    }
}
