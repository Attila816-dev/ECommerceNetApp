using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Cart;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Cart
{
    public class AddCartItemCommandHandlerTest
    {
        private readonly AddCartItemCommandHandler _commandHandler;
        private readonly Mock<ICartRepository> _mockRepository;
        private readonly Mock<ICartUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IValidator<AddCartItemCommand>> _mockValidator;

        public AddCartItemCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICartRepository>();
            _mockUnitOfWork = new Mock<ICartUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.CartRepository).Returns(_mockRepository.Object);
            _mockValidator = new Mock<IValidator<AddCartItemCommand>>();
            _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<AddCartItemCommand>(), CancellationToken.None))
                .ReturnsAsync(new ValidationResult());
            _commandHandler = new AddCartItemCommandHandler(_mockUnitOfWork.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task CallAddCartItem_AddsItemToCart()
        {
            // Arrange
            string testCartId = "test-cart-123";

            var cart = CreateTestCart(testCartId);
            SetupMockRepository(cart);

            _mockUnitOfWork.Setup(x => x.CommitAsync(CancellationToken.None)).Returns(Task.CompletedTask).Verifiable();

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

            _mockUnitOfWork.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task AddItemToCart_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            string testCartId = "test-cart-123";
            var cart = CreateTestCart(testCartId);

            cart.AddItem(1, "Existing Item", Money.From(15.99m), 1);
            SetupMockRepository(cart);

            _mockUnitOfWork.Setup(x => x.CommitAsync(CancellationToken.None)).Returns(Task.CompletedTask).Verifiable();

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

            _mockUnitOfWork.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
        }

        private static CartEntity CreateTestCart(string cartId)
            => CartEntity.Create(cartId);

        private void SetupMockRepository(CartEntity cart)
        {
            _mockRepository.Setup(r => r.GetByIdAsync(cart.Id, CancellationToken.None)).ReturnsAsync(cart);
        }
    }
}