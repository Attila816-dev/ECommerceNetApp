using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Category;
using Moq;
using Shouldly;
using CategoryEntity = ECommerceNetApp.Domain.Entities.Category;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Category
{
    public class DeleteCategoryCommandHandlerTest
    {
        private readonly DeleteCategoryCommandHandler _commandHandler;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;

        public DeleteCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _commandHandler = new DeleteCategoryCommandHandler(_mockCategoryRepository.Object, _mockProductRepository.Object);
        }

        [Fact]
        public async Task DeleteExistingCategory_RemovesCategory()
        {
            // Arrange
            var category = new CategoryEntity(1, "test-category");

            _mockCategoryRepository.Setup(r => r.ExistsAsync(category.Id, CancellationToken.None))
                .ReturnsAsync(true);

            _mockCategoryRepository.Setup(r => r.DeleteAsync(category.Id, CancellationToken.None))
                .Verifiable();

            // Act
            await _commandHandler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

            // Assert
            _mockCategoryRepository.Verify(
                r => r.DeleteAsync(It.Is<int>(c => c == category.Id), CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WithNonExistingCategory_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var categoryId = 2;
            _mockCategoryRepository.Setup(repo => repo.ExistsAsync(categoryId, CancellationToken.None))
                .ReturnsAsync(false);

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(() =>
                _commandHandler.Handle(new DeleteCategoryCommand(categoryId), CancellationToken.None));
        }
    }
}
