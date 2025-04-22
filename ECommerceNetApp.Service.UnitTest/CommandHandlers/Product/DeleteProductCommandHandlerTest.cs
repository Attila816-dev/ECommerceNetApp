using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Product;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Product;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Product
{
    public class DeleteProductCommandHandlerTest
    {
        private readonly DeleteProductCommandHandler _commandHandler;
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;

        public DeleteProductCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.SetupGet(u => u.ProductRepository).Returns(_mockRepository.Object);
            _commandHandler = new DeleteProductCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task DeleteExistingProduct_RemovesProduct()
        {
            // Arrange
            var category = new CategoryEntity(1, "test-category");
            var product = new ProductEntity(1, "test-product", null, null, category, 10, 2);

            _mockRepository.Setup(r => r.ExistsAsync(product.Id, CancellationToken.None))
                .ReturnsAsync(true);

            _mockRepository.Setup(r => r.DeleteAsync(product.Id, CancellationToken.None))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.CommitAsync(CancellationToken.None))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _commandHandler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.DeleteAsync(It.Is<int>(c => c == product.Id), CancellationToken.None),
                Times.Once);

            _mockUnitOfWork.Verify(
                u => u.CommitAsync(CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_WithNonExistingProduct_ShouldThrowInvalidProductException()
        {
            // Arrange
            var productId = 2;
            _mockRepository.Setup(repo => repo.ExistsAsync(productId, CancellationToken.None))
                .ReturnsAsync(false);

            // Act & Assert
            await Should.ThrowAsync<InvalidProductException>(() =>
                _commandHandler.Handle(new DeleteProductCommand(productId), CancellationToken.None));
        }
    }
}
