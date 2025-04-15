using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Cart;
using Moq;
using CartEntity = ECommerceNetApp.Domain.Entities.Cart;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Cart
{
    public class RemoveCartItemCommandHandlerTest
    {
        private readonly RemoveCartItemCommandHandler _commandHandler;
        private readonly Mock<ICartRepository> _mockRepository;

        public RemoveCartItemCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICartRepository>();
            _commandHandler = new RemoveCartItemCommandHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task RemoveItemFromCart_ExistingItem_RemovesItemFromCart()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = new CartEntity(testCartId);

            cart.AddItem(new CartItem(1, "Item 1", new Money(10.99m), 1));
            cart.AddItem(new CartItem(2, "Item 2", new Money(20.99m), 2));

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId, CancellationToken.None))
                .ReturnsAsync(cart);

            // Act
            await _commandHandler.Handle(new RemoveCartItemCommand(testCartId, 1), CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(
                    It.Is<CartEntity>(c => c.Items.Count == 1 && c.Items.Count == 1 && c.Items.First().Id == 2),
                    CancellationToken.None),
                Times.Once);
        }
    }
}
