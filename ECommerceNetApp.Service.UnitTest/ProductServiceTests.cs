using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Product;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation;
using ECommerceNetApp.Service.Queries.Product;
using Moq;
using Shouldly;
using CategoryEntity = ECommerceNetApp.Domain.Entities.Category;

namespace ECommerceNetApp.Service.UnitTest
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _productService = new ProductService(_mockProductRepository.Object, _mockCategoryRepository.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var category = new CategoryEntity(1, "Electronics");
            var products = new List<Product>
            {
                new Product(1, "Laptop", null, null, category, 999.99m, 10),
                new Product(2, "Smartphone", null, null, category, 499.99m, 20),
            };

            _mockProductRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync(new GetAllProductsQuery());

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Id == 1 && p.Name == "Laptop" && p.Price == 999.99m);
            Assert.Contains(result, p => p.Id == 2 && p.Name == "Smartphone" && p.Price == 499.99m);
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_ShouldReturnProductsInCategory()
        {
            // Arrange
            var category = new CategoryEntity(1, "Electronics");
            var products = new List<Product>
            {
                new Product(1, "Laptop", null, null, category, 999.99m, 10),
                new Product(2, "Smartphone", null, null, category, 499.99m, 20),
            };

            _mockProductRepository.Setup(repo => repo.GetProductsByCategoryIdAsync(1))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetProductsByCategoryIdAsync(new GetProductsByCategoryQuery(1));

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(1, p.CategoryId));
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var category = new CategoryEntity(1, "Electronics");
            var product = new Product(1, "Laptop", null, null, category, 999.99m, 10);

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(new GetProductByIdQuery(1));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Laptop", result.Name);
            Assert.Equal(999.99m, result.Price);
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

            var category = new CategoryEntity(1, "Electronics");
            var product = new Product(1, "Laptop", null, null, category, 999.99m, 10);

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            _mockProductRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
                .ReturnsAsync(product);

            // Act
            var command = new CreateProductCommand(
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                productDto.Amount);
            var result = await _productService.AddProductAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Laptop", result.Name);
            Assert.Equal(999.99m, result.Price);

            _mockProductRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task AddProductAsync_WithNonExistingCategory_ShouldThrowArgumentException()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Name = "Laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 999, // Non-existing category ID
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((CategoryEntity?)null);

            // Act & Assert
            var command = new CreateProductCommand(
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                productDto.Amount);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _productService.AddProductAsync(command));

            exception.Message.ShouldContain("Category with ID 999 not found");
        }

        [Fact]
        public async Task UpdateProductAsync_WithNonExistingProduct_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Id = 999,
                Name = "Laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 1,
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new CategoryEntity(1, "Electronics"));

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            var command = new UpdateProductCommand(
                productDto.Id,
                productDto.Name,
                productDto.Description,
                productDto.ImageUrl,
                productDto.CategoryId,
                productDto.Price,
                productDto.Amount);
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _productService.UpdateProductAsync(command));
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

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(category);

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Product(1, "Laptop", null, null, category, 10.0m, 10));

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
                _productService.UpdateProductAsync(command));

            exception.Message.ShouldContain("Product price must be greater than zero");
        }

        [Fact]
        public async Task DeleteProductAsync_WithExistingProduct_ShouldCallDeleteOnRepository()
        {
            // Arrange
            var category = new CategoryEntity(1, "Electronics");
            var product = new Product(1, "Laptop", null, null, category, 10.0m, 10);

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(product);

            // Act
            await _productService.DeleteProductAsync(new DeleteProductCommand(1));

            // Assert
            _mockProductRepository.Verify(repo => repo.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_WithNonExistingProduct_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Should.ThrowAsync<KeyNotFoundException>(() =>
                _productService.DeleteProductAsync(new DeleteProductCommand(999)));
        }
    }
}
