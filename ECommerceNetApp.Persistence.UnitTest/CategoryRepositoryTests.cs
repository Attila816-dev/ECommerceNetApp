using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ECommerceNetApp.Persistence.UnitTest
{
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly ProductCatalogDbContext _dbContext;
        private readonly CategoryRepository _categoryRepository;
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

            _dbContext = new ProductCatalogDbContext(options);
            _categoryRepository = new CategoryRepository(_dbContext);

            // Seed test data.
            SeedTestData();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCategories()
        {
            // Act
            var result = await _categoryRepository.GetAllAsync();

            // Assert
            result.Count().ShouldBe(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnCategory()
        {
            // Act
            var result = await _categoryRepository.GetByIdAsync(1);

            // Assert
            result.ShouldNotBeNull();
            result!.Name.ShouldBe("Electronics");
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewCategory()
        {
            // Arrange
            var newCategory = new Category { Name = "Toys" };

            // Act
            var result = await _categoryRepository.AddAsync(newCategory);

            // Assert
            result.Id.ShouldNotBe(0);
            result.Name.ShouldBe("Toys");

            // Verify it was added to the database.
            var categoryInDb = await _dbContext.Categories.FindAsync(result.Id);
            categoryInDb.ShouldNotBeNull();
            categoryInDb!.Name.ShouldBe("Toys");
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
                    _dbContext.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        private void SeedTestData()
        {
            _dbContext.Categories.Add(new Category { Id = 1, Name = "Electronics" });
            _dbContext.Categories.Add(new Category { Id = 2, Name = "Books", ParentCategoryId = 1 });
            _dbContext.SaveChanges();
        }
    }
}
