using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation;
using Moq;
using Shouldly;

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
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 999.99m, Amount = 10, CategoryId = 1 },
                new Product { Id = 2, Name = "Smartphone", Price = 499.99m, Amount = 20, CategoryId = 1 },
            };

            _mockProductRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Id == 1 && p.Name == "Laptop" && p.Price == 999.99m);
            Assert.Contains(result, p => p.Id == 2 && p.Name == "Smartphone" && p.Price == 499.99m);
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_ShouldReturnProductsInCategory()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 999.99m, Amount = 10, CategoryId = 1 },
                new Product { Id = 2, Name = "Smartphone", Price = 499.99m, Amount = 20, CategoryId = 1 },
            };

            _mockProductRepository.Setup(repo => repo.GetProductsByCategoryIdAsync(1))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetProductsByCategoryIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(1, p.CategoryId));
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Laptop", Price = 999.99m, Amount = 10, CategoryId = 1 };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

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

            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 1,
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Category { Id = 1, Name = "Electronics" });

            _mockProductRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.AddProductAsync(productDto);

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
                .ReturnsAsync((Category?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _productService.AddProductAsync(productDto));

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
                .ReturnsAsync(new Category { Id = 1, Name = "Electronics" });

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _productService.UpdateProductAsync(productDto));
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

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Category { Id = 1, Name = "Electronics" });

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(new Product { Id = 1 });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _productService.UpdateProductAsync(productDto));

            exception.Message.ShouldContain("Product price must be greater than zero");
        }

        [Fact]
        public async Task DeleteProductAsync_WithExistingProduct_ShouldCallDeleteOnRepository()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Laptop" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(product);

            // Act
            await _productService.DeleteProductAsync(1);

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
                _productService.DeleteProductAsync(999));
        }
    }
}
