using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Persistence.UnitTest.Repositories
{
    public class CategoryRepositoryTests : IDisposable
    {
#pragma warning disable CA2213 // Disposable fields should be disposed
        private readonly ProductCatalogDbContext _dbContext;
#pragma warning restore CA2213 // Disposable fields should be disposed
        private readonly ProductCatalogUnitOfWork _productCatalogUnitOfWork;
        private readonly Mock<IDomainEventService> _mockDomainEventService;
        private bool disposedValue;

        public CategoryRepositoryTests()
        {
            // Set up in-memory database for testing.
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

            // Seed test data.
            SeedTestData();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCategories()
        {
            // Act
            var result = await _productCatalogUnitOfWork.CategoryRepository.GetAllAsync(CancellationToken.None);

            // Assert
            result.Count().ShouldBe(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnCategory()
        {
            // Act
            var result = await _productCatalogUnitOfWork.CategoryRepository.GetByIdAsync(1, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result!.Name.ShouldBe("Electronics");
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewCategory()
        {
            // Arrange
            var newCategory = new CategoryEntity("Toys");

            // Act
            await _productCatalogUnitOfWork.CategoryRepository.AddAsync(newCategory, CancellationToken.None);
            await _productCatalogUnitOfWork.SaveChangesAsync(CancellationToken.None);

            // Assert - Verify it was added to the database.
            var categoryInDb = await _dbContext.Categories.FirstAsync(c => c.Name.Equals("Toys", StringComparison.Ordinal), CancellationToken.None);
            categoryInDb.Id.ShouldNotBe(0);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _productCatalogUnitOfWork.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        private void SeedTestData()
        {
            var category1 = new CategoryEntity(1, "Electronics");
            var category2 = new CategoryEntity(2, "Books", parentCategory: category1);

            _dbContext.Categories.Add(category1);
            _dbContext.Categories.Add(category2);
            _dbContext.SaveChanges();
        }
    }
}
