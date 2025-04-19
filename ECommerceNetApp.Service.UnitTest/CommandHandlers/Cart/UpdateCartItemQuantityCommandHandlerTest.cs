using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Cart;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Cart
{
    public class UpdateCartItemQuantityCommandHandlerTest
    {
        private readonly UpdateCartItemQuantityCommandHandler _commandHandler;
        private readonly Mock<ICartRepository> _mockRepository;
        private readonly Mock<ICartUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IValidator<UpdateCartItemQuantityCommand>> _mockValidator;

        public UpdateCartItemQuantityCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICartRepository>();
            _mockUnitOfWork = new Mock<ICartUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.CartRepository).Returns(_mockRepository.Object);
            _mockValidator = new Mock<IValidator<UpdateCartItemQuantityCommand>>();
            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<UpdateCartItemQuantityCommand>(), CancellationToken.None))
                .ReturnsAsync(new ValidationResult());
            _commandHandler = new UpdateCartItemQuantityCommandHandler(_mockUnitOfWork.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task UpdateItemQuantity_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = new CartEntity(testCartId);

            cart.AddItem(new CartItem(1, "Test Item", new Money(10.99m), 1));

            _mockRepository.Setup(r => r.GetByIdAsync(testCartId, CancellationToken.None))
                .ReturnsAsync(cart);

            // Act
            await _commandHandler.Handle(new UpdateCartItemQuantityCommand(testCartId, 1, 5), CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.SaveAsync(
                    It.Is<CartEntity>(c => c.Items.Count == 1 && c.Items.First().Quantity == 5),
                    CancellationToken.None),
                Times.Once);
        }
    }
}
