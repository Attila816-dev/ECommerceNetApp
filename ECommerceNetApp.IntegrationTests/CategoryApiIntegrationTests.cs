using System.Net;
using System.Net.Http.Json;
using ECommerceNetApp.Api;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Persistence.Implementation.Cart;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ECommerceNetApp.IntegrationTests
{
    public class CategoryApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CategoryApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Remove(services.Single(d => d.ServiceType == typeof(CartDbContext)));
                    services.AddScoped(provider =>
                    {
                        return new CartDbContext("Filename=:memory:;Mode=Memory;Cache=Shared");
                    });

                    var optionsConfig = services
                        .Where(r => r.ServiceType.IsGenericType && r.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)).ToArray();

                    foreach (var option in optionsConfig)
                    {
                        services.Remove(option);
                    }

                    services.AddDbContext<ProductCatalogDbContext>(options => options.UseInMemoryDatabase("TestProductCatalogDb"));

                    services.Configure<CartDbOptions>(o => o.SeedSampleData = false);
                    services.Configure<ProductCatalogDbOptions>(o => o.SeedSampleData = false);
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAllCategories_ReturnsCategories()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            await dbContext.Categories.AddRangeAsync(
                new CategoryEntity("Electronics"),
                new CategoryEntity("Books"));
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            response.EnsureSuccessStatusCode();
            var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
            categories.ShouldNotBeNull();
            categories.ShouldContain(c => c.Name == "Electronics");
            categories.ShouldContain(c => c.Name == "Books");
        }

        [Fact]
        public async Task CreateCategory_AddsCategorySuccessfully()
        {
            // Arrange
            var newCategory = new CategoryDto
            {
                Name = "New Category",
            };

            using var content = JsonContent.Create(newCategory);

            // Act
            var response = await _client.PostAsync("/api/categories", content);

            // Assert
            response.EnsureSuccessStatusCode();
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
            var category = await dbContext.Categories.FirstAsync(c => c.Name == "New Category");
            category.ShouldNotBeNull();
        }

        [Fact]
        public async Task UpdateCategory_UpdatesCategorySuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            var category = new CategoryEntity("Old Category");
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();
            dbContext.Entry(category).State = EntityState.Detached;

            var updatedCategory = new CategoryDto
            {
                Id = category.Id,
                Name = "Updated Category",
            };

            using var content = JsonContent.Create(updatedCategory);

            // Act
            var response = await _client.PutAsync($"/api/categories/{category.Id}", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var updated = await dbContext.Categories.FirstAsync(c => c.Id == category.Id);
            updated.Name.ShouldBe("Updated Category");
        }

        [Fact]
        public async Task DeleteCategory_RemovesCategorySuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            var category = new CategoryEntity("Category to Delete");
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/categories/{category.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var exists = await dbContext.Categories.AnyAsync(c => c.Id == category.Id);
            exists.ShouldBeFalse();
        }

        [Fact]
        public async Task CreateCategory_WithValidData_ReturnsCreated()
        {
            // Arrange
            var newCategory = new CategoryDto
            {
                Name = "Test Category " + Guid.NewGuid(),
                ImageUrl = "http://example.com/image.jpg",
                ParentCategoryId = null,
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/categories", newCategory);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task CreateCategory_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidCategory = new CategoryDto
            {
                // Missing required Name
                ImageUrl = "http://example.com/image.jpg",
                ParentCategoryId = null,
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/categories", invalidCategory);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCategory_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/categories/999999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
