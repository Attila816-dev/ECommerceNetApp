using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Implementation.QueryHandlers.Cart;
using ECommerceNetApp.Service.Queries.Cart;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.QueryHandlers.Cart
{
    public class GetCartTotalQueryHandlerTest
    {
        private readonly GetCartTotalQueryHandler _queryHandler;
        private readonly Mock<ICartRepository> _mockRepository;
        private readonly Mock<ICartUnitOfWork> _mockUnitOfWork;

        public GetCartTotalQueryHandlerTest()
        {
            // Initialize the command handler with necessary dependencies.
            _mockRepository = new Mock<ICartRepository>();
            _mockUnitOfWork = new Mock<ICartUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.CartRepository).Returns(_mockRepository.Object);
            _queryHandler = new GetCartTotalQueryHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetCartTotal_ReturnsExpectedValue()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = CartEntity.Create(testCartId);

            cart.AddItem(1, "Item 1", new Money(10.99m), 1);
            cart.AddItem(2, "Item 2", new Money(20.99m), 2);

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId, CancellationToken.None))
                .ReturnsAsync(cart);

            // Act
            var cartTotal = await _queryHandler.Handle(new GetCartTotalQuery(testCartId), CancellationToken.None);

            // Assert
            cartTotal.ShouldBe(52.97m); // 10.99 + (20.99 * 2)
        }

        [Fact]
        public async Task GetCartTotalWithInvalidCart_ReturnsNull()
        {
            // Arrange
            string testCartId = "test-cart-123";
            _mockRepository.Setup(r => r.GetByIdAsync(testCartId, CancellationToken.None))
                .ReturnsAsync((CartEntity?)null);

            // Act
            var cartTotal = await _queryHandler.Handle(new GetCartTotalQuery(testCartId), CancellationToken.None);

            // Assert
            cartTotal.ShouldBeNull();
        }
    }
}
