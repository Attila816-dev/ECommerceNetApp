using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Implementation.Mappers;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Cart;
using ECommerceNetApp.Service.Queries.Cart;
using Moq;
using Shouldly;
using CartEntity = ECommerceNetApp.Domain.Entities.Cart;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Cart
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
                .ReturnsAsync((CartEntity?)null);

            // Act
            var result = await _queryHandler.Handle(new GetCartItemsQuery(testCartId), CancellationToken.None);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetCartItems_WithItems_ReturnsItems()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = new CartEntity(testCartId);

            cart.AddItem(new CartItem(1, "Test Item", new Money(10.99m), 2));

            _mockRepository
                .Setup(r => r.GetByIdAsync(testCartId, CancellationToken.None))
                .ReturnsAsync(cart);

            // Act
            var result = await _queryHandler.Handle(new GetCartItemsQuery(testCartId), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result.First().Name.ShouldBe("Test Item");
            result.First().Quantity.ShouldBe(2);
        }
    }
}
