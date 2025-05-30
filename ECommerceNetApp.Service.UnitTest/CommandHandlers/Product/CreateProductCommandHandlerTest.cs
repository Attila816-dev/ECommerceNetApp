using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Category;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Product;
using ECommerceNetApp.Service.UnitTest.Extensions;
using Moq;
using Moq.EntityFrameworkCore;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Product
{
    public class CreateProductCommandHandlerTest
    {
        private readonly CreateProductCommandHandler _commandHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;

        public CreateProductCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockDbContext = MockProductCatalogDbContextFactory.Create().DbContext;
            _commandHandler = new CreateProductCommandHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task AddProductAsync_WithValidData_ShouldAddProduct()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Name = "Laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 1,
            };

            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(999.99m), 10, 1);

            var categories = new List<CategoryEntity> { category }.AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            var products = new List<ProductEntity>().AsQueryable();
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(products);

            // Act
            var command = new CreateProductCommand(
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                null,
                productDto.Amount);
            var result = await _commandHandler.HandleAsync(command, CancellationToken.None);

            // Assert
            _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddProductAsync_WithNonExistingCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Name = "Laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 999, // Non-existing category ID
            };

            var categories = new List<CategoryEntity>().AsQueryable();
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(categories);

            var products = new List<ProductEntity>().AsQueryable();
            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(products);

            // Act & Assert
            var command = new CreateProductCommand(
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                null,
                productDto.Amount);
            var exception = await Should.ThrowAsync<InvalidCategoryException>(() =>
                _commandHandler.HandleAsync(command, CancellationToken.None));

            exception.Message.ShouldContain("Category with ID 999 not found");
        }
    }
}
