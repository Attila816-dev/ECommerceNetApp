using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Implementation.QueryHandlers;
using ECommerceNetApp.Service.Queries;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers
{
    public class GetCartTotalQueryHandlerTest
    {
        private readonly GetCartTotalQueryHandler _queryHandler;
        private readonly Mock<ICartRepository> _mockRepository;

        public GetCartTotalQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies.
            _mockRepository = new Mock<ICartRepository>();
            _queryHandler = new GetCartTotalQueryHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task GetCartTotal_ReturnsExpectedValue()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = new Cart(testCartId);

            cart.AddItem(new CartItem(1, "Item 1", new Money(10.99m), 1));
            cart.AddItem(new CartItem(2, "Item 2", new Money(20.99m), 2));

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId, CancellationToken.None))
                .ReturnsAsync(cart);

            // Act
            var cartTotal = await _queryHandler.Handle(new GetCartTotalQuery(testCartId), CancellationToken.None);

            // Assert
            cartTotal.ShouldBe(52.97m); // 10.99 + (20.99 * 2)
        }
    }
}
