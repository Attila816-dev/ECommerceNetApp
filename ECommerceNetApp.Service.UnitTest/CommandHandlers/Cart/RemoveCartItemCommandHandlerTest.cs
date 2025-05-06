using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Cart;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Cart
{
    public class RemoveCartItemCommandHandlerTest
    {
        private readonly RemoveCartItemCommandHandler _commandHandler;
        private readonly Mock<ICartRepository> _mockRepository;
        private readonly Mock<ICartUnitOfWork> _mockUnitOfWork;

        public RemoveCartItemCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICartRepository>();
            _mockUnitOfWork = new Mock<ICartUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.CartRepository).Returns(_mockRepository.Object);
            _commandHandler = new RemoveCartItemCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task RemoveItemFromCart_ExistingItem_RemovesItemFromCart()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = CartEntity.Create(testCartId);

            cart.AddItem(1, "Item 1", Money.From(10.99m), 1);
            cart.AddItem(2, "Item 2", Money.From(20.99m), 2);

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId, CancellationToken.None))
                .ReturnsAsync(cart);

            // Act
            await _commandHandler.HandleAsync(new RemoveCartItemCommand(testCartId, 1), CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(
                    It.Is<CartEntity>(c => c.Items.Count == 1 && c.Items.Count == 1 && c.Items.First().Id == 2),
                    CancellationToken.None),
                Times.Once);
        }
    }
}
