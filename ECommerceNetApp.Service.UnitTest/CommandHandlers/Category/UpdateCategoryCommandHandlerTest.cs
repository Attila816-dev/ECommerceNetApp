using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Category;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Category
{
    public class UpdateCategoryCommandHandlerTest
    {
        private readonly UpdateCategoryCommandHandler _commandHandler;
        private readonly Mock<ICategoryRepository> _mockRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;

        public UpdateCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.CategoryRepository).Returns(_mockRepository.Object);
            _commandHandler = new UpdateCategoryCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WithValidData_ShouldUpdateCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto { Id = 1, Name = "Electronics Updated" };

            _mockRepository.Setup(repo => repo.GetByIdAsync(
                It.Is<int>(id => id == 1),
                It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CategoryEntity(categoryDto.Id, "Electronics"));

            _mockRepository.Setup(c => c.Update(It.Is<CategoryEntity>(c => c.Name == categoryDto.Name && c.Id == 1)))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var updateCategoryCommand = new UpdateCategoryCommand(categoryDto.Id, categoryDto.Name, null, null);
            await _commandHandler.Handle(updateCategoryCommand, CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.Update(It.Is<CategoryEntity>(c => c.Name == categoryDto.Name)),
                Times.Once);

            _mockUnitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WithNonExistingCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var categoryDto = new CategoryDto { Id = 99, Name = "Electronics Updated" };

            _mockRepository.Setup(repo => repo.GetByIdAsync(
                It.Is<int>(id => id == 1),
                It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CategoryEntity(categoryDto.Id, "Electronics"));

            _mockRepository.Setup(c => c.Update(It.Is<CategoryEntity>(c => c.Name == categoryDto.Name && c.Id == 1)))
                .Verifiable();

            // Act
            var updateCategoryCommand = new UpdateCategoryCommand(categoryDto.Id, categoryDto.Name, null, null);

            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _commandHandler.Handle(updateCategoryCommand, CancellationToken.None);
            });

            // Assert
            _mockRepository.Verify(
                r => r.Update(It.Is<CategoryEntity>(c => c.Name == categoryDto.Name)),
                Times.Never);
        }
    }
}
