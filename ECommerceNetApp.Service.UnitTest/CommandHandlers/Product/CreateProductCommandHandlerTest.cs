using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Product;
using ECommerceNetApp.Service.Implementation.Mappers.Product;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Product
{
    public class CreateProductCommandHandlerTest
    {
        private readonly CreateProductCommandHandler _commandHandler;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly ProductMapper _productMapper;

        public CreateProductCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.SetupGet(x => x.ProductRepository).Returns(_mockProductRepository.Object);
            _mockUnitOfWork.SetupGet(x => x.CategoryRepository).Returns(_mockCategoryRepository.Object);
            _productMapper = new ProductMapper();

            _commandHandler = new CreateProductCommandHandler(_mockUnitOfWork.Object, _productMapper);
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

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(
                category.Id,
                It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                CancellationToken.None))
                .ReturnsAsync(category);

            _mockProductRepository.Setup(repo => repo.AddAsync(It.Is<ProductEntity>(p => p.Name == productDto.Name), CancellationToken.None))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.CommitAsync(CancellationToken.None))
                .Returns(Task.CompletedTask)
                .Verifiable();

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
            _mockProductRepository.Verify(
                repo => repo.AddAsync(It.Is<ProductEntity>(p => p.Name == productDto.Name), CancellationToken.None),
                Times.Once);

            _mockUnitOfWork.Verify(x => x.CommitAsync(CancellationToken.None), Times.Once);
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

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(
                999,
                It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                CancellationToken.None))
                .ReturnsAsync((CategoryEntity?)null);

            // Act & Assert
            var command = new CreateProductCommand(
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                null,
                productDto.Amount);
            var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
                _commandHandler.HandleAsync(command, CancellationToken.None));

            exception.Message.ShouldContain("Category with ID 999 not found");
        }
    }
}
