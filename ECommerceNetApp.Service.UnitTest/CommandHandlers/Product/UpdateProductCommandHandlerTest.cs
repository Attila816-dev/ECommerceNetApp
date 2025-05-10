using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Exceptions.Product;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Product;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Product
{
    public class UpdateProductCommandHandlerTest
    {
        private readonly UpdateProductCommandHandler _commandHandler;
        private readonly Mock<ProductCatalogDbContext> _mockDbContext;
        private readonly Mock<IEventBus> _mockEventBus;

        public UpdateProductCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockEventBus = new Mock<IEventBus>();
            _mockDbContext = new Mock<ProductCatalogDbContext>(new DbContextOptions<ProductCatalogDbContext>(), _mockEventBus.Object);
            _commandHandler = new UpdateProductCommandHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidData_ShouldUpdateProduct()
        {
            // Arrange
            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var productDto = new ProductDto
            {
                Id = 2,
                Name = "Updated Laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 1,
            };

            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10m), 2, productDto.Id);

            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity> { product }.AsQueryable());
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(new List<CategoryEntity> { category }.AsQueryable());

            // Act
            var updateProductCommand = new UpdateProductCommand(
                productDto.Id,
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                null,
                productDto.Amount);
            await _commandHandler.HandleAsync(updateProductCommand, CancellationToken.None);

            // Assert
            _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithNonExistingProduct_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var category = CategoryEntity.Create("Electronics", null, null, 1);
            var productDto = new ProductDto
            {
                Id = 999,
                Name = "Laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 1,
            };

            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity>().AsQueryable());
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(new List<CategoryEntity> { category }.AsQueryable());

            // Act & Assert
            var command = new UpdateProductCommand(
                productDto.Id,
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                null,
                productDto.Amount);
            await Should.ThrowAsync<InvalidProductException>(() =>
                _commandHandler.HandleAsync(command, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateProductAsync_WithInvalidPrice_ShouldThrowArgumentException()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Id = 1,
                Name = "Laptop",
                Price = -10.0m, // Invalid price
                Amount = 10,
                CategoryId = 1,
            };

            var category = CategoryEntity.Create("Electronics", null, null, 1);

            var product = ProductEntity.Create("Laptop", null, null, category, Money.From(10.0m), 10, 1);

            _mockDbContext.SetupGet(c => c.Products).ReturnsDbSet(new List<ProductEntity> { product }.AsQueryable());
            _mockDbContext.SetupGet(c => c.Categories).ReturnsDbSet(new List<CategoryEntity> { category }.AsQueryable());

            // Act & Assert
            var command = new UpdateProductCommand(
                productDto.Id,
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                null,
                productDto.Amount);

            var exception = await Should.ThrowAsync<ArgumentException>(() =>
                _commandHandler.HandleAsync(command, CancellationToken.None));

            exception.Message.ShouldContain("Price cannot be negative");
        }
    }
}
