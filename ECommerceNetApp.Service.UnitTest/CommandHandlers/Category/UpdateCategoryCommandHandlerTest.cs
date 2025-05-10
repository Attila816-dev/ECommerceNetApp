using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Category;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Category;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Category
{
    public class UpdateCategoryCommandHandlerTest
    {
        private readonly UpdateCategoryCommandHandler _commandHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;
        private readonly Mock<IEventBus> _mockEventBus;

        public UpdateCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockEventBus = new Mock<IEventBus>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), _mockEventBus.Object);
            _commandHandler = new UpdateCategoryCommandHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WithValidData_ShouldUpdateCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto { Id = 1, Name = "Electronics Updated" };
            var category = CategoryEntity.Create("Electronics", null, null, categoryDto.Id);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var updateCategoryCommand = new UpdateCategoryCommand(categoryDto.Id, categoryDto.Name, null, null);
            await _commandHandler.HandleAsync(updateCategoryCommand, CancellationToken.None);

            // Assert
            _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WithNonExistingCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var categoryDto = new CategoryDto { Id = 99, Name = "Electronics Updated" };
            var category = CategoryEntity.Create("Electronics", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var updateCategoryCommand = new UpdateCategoryCommand(categoryDto.Id, categoryDto.Name, null, null);

            await Should.ThrowAsync<InvalidCategoryException>(async () =>
            {
                await _commandHandler.HandleAsync(updateCategoryCommand, CancellationToken.None);
            });

            // Assert
            _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
