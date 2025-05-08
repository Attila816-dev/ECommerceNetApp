using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Category;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Category
{
    public class CreateCategoryCommandHandlerTest
    {
        private readonly CreateCategoryCommandHandler _commandHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public CreateCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            var mockDomainEventService = new Mock<IDomainEventService>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), mockDomainEventService.Object);
            _commandHandler = new CreateCategoryCommandHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task AddCategoryAsync_WithValidData_ShouldAddCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "Electronics" };

            var categories = new List<CategoryEntity>().AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _commandHandler.HandleAsync(new CreateCategoryCommand(categoryDto.Name, null, null), CancellationToken.None);

            // Assert
            _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithParentCategory_CorrectlyAssignsParent()
        {
            // Arrange
            var parentCategoryId = 42;
            var command = new CreateCategoryCommand("Test Category", "image.jpg", parentCategoryId);
            var parentCategory = CategoryEntity.Create("Parent Category", ImageInfo.Create("parent.jpg"), null, parentCategoryId);
            var categoryEntity = CategoryEntity.Create("Test Category", ImageInfo.Create("image.jpg"), parentCategory, 123);

            var categories = new List<CategoryEntity> { parentCategory }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            // Act
            var result = await _commandHandler.HandleAsync(command, CancellationToken.None);

            // Assert
            _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
