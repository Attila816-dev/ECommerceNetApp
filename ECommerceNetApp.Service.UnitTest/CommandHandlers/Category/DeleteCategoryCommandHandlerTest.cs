using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Category;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Category
{
    public class DeleteCategoryCommandHandlerTest
    {
        private readonly DeleteCategoryCommandHandler _commandHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public DeleteCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            var mockDomainEventService = new Mock<IDomainEventService>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), mockDomainEventService.Object);
            _commandHandler = new DeleteCategoryCommandHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task DeleteExistingCategory_RemovesCategory()
        {
            // Arrange
            var category = CategoryEntity.Create("test-category", null, null, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            var products = new List<ProductEntity>().AsQueryable();
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(products);

            // Act
            await _commandHandler.HandleAsync(new DeleteCategoryCommand(category.Id), CancellationToken.None);

            // Assert
            _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
