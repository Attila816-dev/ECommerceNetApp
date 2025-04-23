using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Product;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Product
{
    public class UpdateProductCommandHandlerTest
    {
        private readonly UpdateProductCommandHandler _commandHandler;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;

        public UpdateProductCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.SetupGet(u => u.ProductRepository).Returns(_mockProductRepository.Object);
            _mockUnitOfWork.SetupGet(u => u.CategoryRepository).Returns(_mockCategoryRepository.Object);

            _commandHandler = new UpdateProductCommandHandler(_mockUnitOfWork.Object);
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

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(
                It.Is<int>(id => id == productDto.Id),
                It.IsAny<Func<IQueryable<ProductEntity>, IQueryable<ProductEntity>>?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductEntity(productDto.Id, "Laptop", null, null, category, new Money(10m, null), 2));

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(
                It.Is<int>(id => id == category.Id),
                It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _mockProductRepository.Setup(c => c.Update(It.Is<ProductEntity>(c => c.Name == productDto.Name && c.Id == productDto.Id)))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

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
            await _commandHandler.Handle(updateProductCommand, CancellationToken.None);

            // Assert
            _mockProductRepository.Verify(
                r => r.Update(It.Is<ProductEntity>(c => c.Name == productDto.Name)),
                Times.Once);

            _mockUnitOfWork.Verify(
                r => r.CommitAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithNonExistingProduct_ShouldThrowInvalidOperationException()
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

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(
                1,
                It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                CancellationToken.None))
                .ReturnsAsync(new CategoryEntity(1, "Electronics"));

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(
                productDto.Id,
                It.IsAny<Func<IQueryable<ProductEntity>, IQueryable<ProductEntity>>?>(),
                CancellationToken.None))
                .ReturnsAsync((ProductEntity?)null);

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

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(
                1,
                It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                CancellationToken.None))
                .ReturnsAsync(category);

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(
                1,
                It.IsAny<Func<IQueryable<ProductEntity>, IQueryable<ProductEntity>>?>(),
                CancellationToken.None))
                .ReturnsAsync(new ProductEntity(1, "Laptop", null, null, category, new Money(10.0m, null), 10));

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
                _commandHandler.Handle(command, CancellationToken.None));

            exception.Message.ShouldContain("Price cannot be negative");
        }
    }
}
