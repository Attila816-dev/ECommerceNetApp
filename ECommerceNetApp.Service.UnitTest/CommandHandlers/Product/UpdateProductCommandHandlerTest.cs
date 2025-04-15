using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Product;
using Moq;
using Shouldly;
using CategoryEntity = ECommerceNetApp.Domain.Entities.Category;
using ProductEntity = ECommerceNetApp.Domain.Entities.Product;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Product
{
    public class UpdateProductCommandHandlerTest
    {
        private readonly UpdateProductCommandHandler _commandHandler;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;

        public UpdateProductCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _commandHandler = new UpdateProductCommandHandler(_mockProductRepository.Object, _mockCategoryRepository.Object);
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidData_ShouldUpdateProduct()
        {
            // Arrange
            var category = new CategoryEntity(1, "Electronics");
            var productDto = new ProductDto
            {
                Id = 2,
                Name = "Updated Laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 1,
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id == productDto.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductEntity(productDto.Id, "Laptop", null, null, category, 10m, 2));

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id == category.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _mockProductRepository.Setup(c => c.UpdateAsync(It.Is<ProductEntity>(c => c.Name == productDto.Name && c.Id == productDto.Id), CancellationToken.None))
                .Verifiable();

            // Act
            var updateProductCommand = new UpdateProductCommand(
                productDto.Id,
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                productDto.Amount);
            await _commandHandler.Handle(updateProductCommand, CancellationToken.None);

            // Assert
            _mockProductRepository.Verify(
                r => r.UpdateAsync(
                    It.Is<ProductEntity>(c => c.Name == productDto.Name),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithNonExistingProduct_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var category = new CategoryEntity(1, "Electronics");
            var productDto = new ProductDto
            {
                Id = 999,
                Name = "Laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 1,
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(1, CancellationToken.None))
                .ReturnsAsync(new CategoryEntity(1, "Electronics"));

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productDto.Id, CancellationToken.None))
                           .ReturnsAsync((ProductEntity?)null);

            // Act & Assert
            var command = new UpdateProductCommand(
                productDto.Id,
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                productDto.Amount);
            await Should.ThrowAsync<InvalidOperationException>(() =>
                _commandHandler.Handle(command, CancellationToken.None));
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

            var category = new CategoryEntity(1, "Electronics");

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(1, CancellationToken.None)).ReturnsAsync(category);

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1, CancellationToken.None))
                           .ReturnsAsync(new ProductEntity(1, "Laptop", null, null, category, 10.0m, 10));

            // Act & Assert
            var command = new UpdateProductCommand(
                productDto.Id,
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                productDto.Amount);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _commandHandler.Handle(command, CancellationToken.None));

            exception.Message.ShouldContain("Product price must be greater than zero");
        }
    }
}
