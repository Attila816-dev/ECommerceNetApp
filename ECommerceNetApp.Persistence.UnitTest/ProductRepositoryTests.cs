using ECommerceNetApp.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ECommerceNetApp.Persistence.UnitTest
{
    public class ProductRepositoryTests : IDisposable
    {
        private readonly ProductCatalogDbContext _dbContext;
        private readonly ProductRepository _productRepository;
        private bool disposedValue;

        public ProductRepositoryTests()
        {
            // Set up in-memory database for testing
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ProductCatalogDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            _dbContext = new ProductCatalogDbContext(options);
            _productRepository = new ProductRepository(_dbContext);

            // Seed test data
            SeedTestData();
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Act
            var result = await _productRepository.GetAllAsync();

            // Assert
            result.Count().ShouldBe(3);
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_ShouldReturnProductsInCategory()
        {
            // Act
            var electronicsProducts = await _productRepository.GetProductsByCategoryIdAsync(1);
            var booksProducts = await _productRepository.GetProductsByCategoryIdAsync(2);

            // Assert
            electronicsProducts.Count().ShouldBe(2);
            booksProducts.Count().ShouldBe(1);
            electronicsProducts.ShouldAllBe(p => p.CategoryId == 1);
            booksProducts.ShouldAllBe(p => p.CategoryId == 2);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
        {
            // Act
            var result = await _productRepository.GetByIdAsync(1);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Laptop");
            result.Price.ShouldBe(999.99m);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _productRepository.GetByIdAsync(999);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task AddProductAsync_ShouldAddNewProduct()
        {
            // Arrange
            var newProduct = new Product
            {
                Name = "Tablet",
                Description = "Compact tablet",
                Price = 299.99m,
                Amount = 15,
                CategoryId = 1,
            };

            // Act
            var result = await _productRepository.AddAsync(newProduct);

            // Assert
            result.Id.ShouldNotBe(0);
            result.Name.ShouldBe("Tablet");

            // Verify it was added to the database
            var productInDb = await _dbContext.Products.FindAsync(result.Id);
            productInDb.ShouldNotBeNull();
            productInDb.Name.ShouldBe("Tablet");
            productInDb.Price.ShouldBe(299.99m);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldUpdateExistingProduct()
        {
            // Arrange
            var product = await _dbContext.Products.FindAsync(1);
            product.ShouldNotBeNull();
            product.Name = "Updated Laptop";
            product.Price = 1099.99m;

            // Act
            await _productRepository.UpdateAsync(product);

            // Assert
            var updatedProduct = await _dbContext.Products.FindAsync(1);
            updatedProduct.ShouldNotBeNull();
            updatedProduct.Name.ShouldBe("Updated Laptop");
            updatedProduct.Price.ShouldBe(1099.99m);
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldRemoveProduct()
        {
            // Act
            await _productRepository.DeleteAsync(1);

            // Assert
            var deletedProduct = await _dbContext.Products.FindAsync(1);
            deletedProduct.ShouldBeNull();

            // Verify we now have one less product
            var remainingProducts = await _productRepository.GetAllAsync();
            remainingProducts.Count().ShouldBe(2);
        }

        [Fact]
        public async Task GetProductsIncludingCategory_ShouldIncludeCategoryData()
        {
            // Act
            var products = await _productRepository.GetAllAsync();
            var product = await _productRepository.GetByIdAsync(1);

            // Assert
            // Check that Category navigation property is loaded
            products.ShouldAllBe(p => p.Category != null);
            product.ShouldNotBeNull();
            product.Category.ShouldNotBeNull();
            product.Category.Name.ShouldBe("Electronics");
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _dbContext.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        private void SeedTestData()
        {
            // Add categories
            var electronicsCategory = new Category { Id = 1, Name = "Electronics" };
            var booksCategory = new Category { Id = 2, Name = "Books" };

            _dbContext.Categories.Add(electronicsCategory);
            _dbContext.Categories.Add(booksCategory);
            _dbContext.SaveChanges();

            // Add products
            _dbContext.Products.Add(new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "Powerful laptop",
                Price = 999.99m,
                Amount = 10,
                CategoryId = 1,
            });

            _dbContext.Products.Add(new Product
            {
                Id = 2,
                Name = "Smartphone",
                Description = "Latest model",
                Price = 499.99m,
                Amount = 20,
                CategoryId = 1,
            });

            _dbContext.Products.Add(new Product
            {
                Id = 3,
                Name = "Programming Book",
                Description = "Learn to code",
                Price = 39.99m,
                Amount = 50,
                CategoryId = 2,
            });

            _dbContext.SaveChanges();
        }
    }
}
