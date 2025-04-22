using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Persistence.UnitTest.Repositories
{
    public class ProductRepositoryTests : IDisposable
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private readonly ProductCatalogDbContext _dbContext;
#pragma warning restore CA2213 // Disposable fields should be disposed
        private readonly ProductCatalogUnitOfWork _productCatalogUnitOfWork;
        private readonly Mock<IDomainEventService> _mockDomainEventService;

        private bool disposedValue;
        private CategoryEntity _electronicsCategory = new CategoryEntity("Electronics", null, null);
        private CategoryEntity _booksCategory = new CategoryEntity("Books", null, null);
        private ProductEntity? _laptopProduct;
        private ProductEntity? _smartPhoneProduct;
        private ProductEntity? _bookProduct;

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

            _mockDomainEventService = new Mock<IDomainEventService>();
            _dbContext = new ProductCatalogDbContext(options);
            _productCatalogUnitOfWork = new ProductCatalogUnitOfWork(_dbContext, _mockDomainEventService.Object);

            // Seed test data
            SeedTestData();
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Act
            var result = await _productCatalogUnitOfWork.ProductRepository.GetAllAsync(CancellationToken.None);

            // Assert
            result.Count().ShouldBe(3);
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_ShouldReturnProductsInCategory()
        {
            // Act
            var electronicsProducts = await _productCatalogUnitOfWork.ProductRepository.GetProductsByCategoryIdAsync(_electronicsCategory.Id, CancellationToken.None);
            var booksProducts = await _productCatalogUnitOfWork.ProductRepository.GetProductsByCategoryIdAsync(_booksCategory.Id, CancellationToken.None);

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
            var result = await _productCatalogUnitOfWork.ProductRepository.GetByIdAsync(_laptopProduct!.Id, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Laptop");
            result.Price.Amount.ShouldBe(999.99m);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _productCatalogUnitOfWork.ProductRepository.GetByIdAsync(999, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task AddProductAsync_ShouldAddNewProduct()
        {
            // Arrange
            var newProduct = new ProductEntity("Tablet", "Compact tablet", null, _electronicsCategory, new Money(299.99m, null), 15);

            // Act
            await _productCatalogUnitOfWork.ProductRepository.AddAsync(newProduct, CancellationToken.None);
            await _productCatalogUnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Assert - Verify it was added to the database
            var productInDb = await _dbContext.Products.FirstAsync(p => p.Name == newProduct.Name);
            productInDb.ShouldNotBeNull();
            productInDb.Description.ShouldBe(newProduct.Description);
            productInDb.Price.Amount.ShouldBe(299.99m);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldUpdateExistingProduct()
        {
            // Arrange
            var product = await _dbContext.Products.FindAsync(_laptopProduct!.Id);
            product.ShouldNotBeNull();
            product.UpdateName("Updated Laptop");
            product.UpdatePrice(new Money(1099.99m, "EUR"));

            // Act
            _productCatalogUnitOfWork.ProductRepository.Update(product);
            await _productCatalogUnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Assert
            var updatedProduct = await _dbContext.Products.FindAsync(1);
            updatedProduct.ShouldNotBeNull();
            updatedProduct.Name.ShouldBe("Updated Laptop");
            updatedProduct.Price.Amount.ShouldBe(1099.99m);
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldRemoveProduct()
        {
            // Act
            await _productCatalogUnitOfWork.ProductRepository.DeleteAsync(1, CancellationToken.None);
            await _productCatalogUnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Assert
            var deletedProduct = await _dbContext.Products.FindAsync(_laptopProduct!.Id);
            deletedProduct.ShouldBeNull();

            // Verify we now have one less product
            var remainingProducts = await _productCatalogUnitOfWork.ProductRepository.GetAllAsync(CancellationToken.None);
            remainingProducts.Count().ShouldBe(2);
        }

        [Fact]
        public async Task GetProductsIncludingCategory_ShouldIncludeCategoryData()
        {
            // Act
            var products = await _productCatalogUnitOfWork.ProductRepository.GetAllAsync(CancellationToken.None);
            var product = await _productCatalogUnitOfWork.ProductRepository.GetByIdAsync(_laptopProduct!.Id, CancellationToken.None);

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
                    _productCatalogUnitOfWork.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        private void SeedTestData()
        {
            // Add categories
            _dbContext.Categories.Add(_electronicsCategory);
            _dbContext.Categories.Add(_booksCategory);
            _dbContext.SaveChanges();

            // Add products
            _laptopProduct = new ProductEntity("Laptop", "Powerful laptop", null, _electronicsCategory, new Money(999.99m, null), 10);
            _smartPhoneProduct = new ProductEntity("Smartphone", "Latest model", null, _electronicsCategory, new Money(499.99m, null), 20);

            _dbContext.Products.Add(_laptopProduct);
            _dbContext.Products.Add(_smartPhoneProduct);

            _bookProduct = new ProductEntity("Programming Book", "Learn to code", null, _booksCategory, new Money(39.99m, null), 50);
            _dbContext.Products.Add(_bookProduct);

            _dbContext.SaveChanges();
        }
    }
}
