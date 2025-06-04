using System.Net;
using System.Net.Http.Json;
using ECommerceNetApp.Api;
using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Domain.ValueObjects;
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
                    services.AddSingleton<ICartDbContextFactory>(new CartDbContextFactory("Filename=:memory:;Mode=Memory;Cache=Shared"));

                    var optionsConfig = services
                        .Where(r => r.ServiceType.IsGenericType && r.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)).ToArray();

                    foreach (var option in optionsConfig)
                    {
                        services.Remove(option);
                    }

                    services.AddDbContext<ProductCatalogDbContext>(options => options.UseInMemoryDatabase("TestProductCatalogDb"));

                    services.Configure<CartDbOptions>(o => o.SeedSampleData = false);
                    services.Configure<ProductCatalogDbOptions>(o =>
                    {
                        o.EnableDatabaseMigration = false;
                        o.SeedSampleData = false;
                    });
                    services.Configure<EventBusOptions>(o => o.Type = "InMemory");
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
                CategoryEntity.Create("Electronics", null, null),
                CategoryEntity.Create("Books", null, null));
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            response.EnsureSuccessStatusCode();
            var categoriesWithLinks = await response.Content.ReadFromJsonAsync<CollectionLinkedResourceDto<CategoryDto>>();
            categoriesWithLinks.ShouldNotBeNull();
            categoriesWithLinks.Items.ShouldNotBeNull();
            categoriesWithLinks.Items.ShouldContain(c => c.Name == "Electronics");
            categoriesWithLinks.Items.ShouldContain(c => c.Name == "Books");
        }

        [Fact]
        public async Task CreateCategory_AddsCategorySuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var newCategory = new CategoryDto
            {
                Name = "New Category",
            };

            using var content = JsonContent.Create(newCategory);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.PostAsync("/api/categories", content);

            // Assert
            response.EnsureSuccessStatusCode();
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

            var category = CategoryEntity.Create("Old Category", null, null);
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();
            dbContext.Entry(category).State = EntityState.Detached;

            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var updatedCategory = new CategoryDto
            {
                Id = category.Id,
                Name = "Updated Category",
            };

            using var content = JsonContent.Create(updatedCategory);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
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

            var category = CategoryEntity.Create("Category to Delete", null, null);
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();

            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
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
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var newCategory = new CategoryDto
            {
                Name = "Test Category " + Guid.NewGuid(),
                ImageUrl = "http://example.com/image.jpg",
                ParentCategoryId = null,
            };

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.PostAsJsonAsync("/api/categories", newCategory);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task CreateCategory_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var invalidCategory = new CategoryDto
            {
                // Missing required Name
                ImageUrl = "http://example.com/image.jpg",
                ParentCategoryId = null,
            };

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.PostAsJsonAsync("/api/categories", invalidCategory);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCategory_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.GetAsync("/api/categories/999999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCategoriesByParentId_WithValidParentId_ReturnsSubcategories()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            var parentCategory = CategoryEntity.Create("Parent Category", null, null);
            await dbContext.Categories.AddAsync(parentCategory);
            await dbContext.SaveChangesAsync();

            var childCategory1 = CategoryEntity.Create("Child 1", null, parentCategory);
            var childCategory2 = CategoryEntity.Create("Child 2", null, parentCategory);
            await dbContext.Categories.AddRangeAsync(childCategory1, childCategory2);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/categories/by-parent?parentCategoryId={parentCategory.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var categoriesWithLinks = await response.Content.ReadFromJsonAsync<CollectionLinkedResourceDto<CategoryDto>>();
            categoriesWithLinks.ShouldNotBeNull();
            categoriesWithLinks.Items.Count().ShouldBe(2);
            categoriesWithLinks.Items.ShouldContain(c => c.Name == "Child 1");
            categoriesWithLinks.Items.ShouldContain(c => c.Name == "Child 2");
        }

        [Fact]
        public async Task GetCategoriesByParentId_WithNullParentId_ReturnsRootCategories()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            var rootCategory1 = CategoryEntity.Create("Root 1", null, null);
            var rootCategory2 = CategoryEntity.Create("Root 2", null, null);
            await dbContext.Categories.AddRangeAsync(rootCategory1, rootCategory2);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/categories/by-parent?parentCategoryId=");

            // Assert
            response.EnsureSuccessStatusCode();
            var categoriesWithLinks = await response.Content.ReadFromJsonAsync<CollectionLinkedResourceDto<CategoryDto>>();
            categoriesWithLinks.ShouldNotBeNull();
            categoriesWithLinks.Items.ShouldContain(c => c.Name == "Root 1");
            categoriesWithLinks.Items.ShouldContain(c => c.Name == "Root 2");
        }

        [Fact]
        public async Task GetCategoryById_WithValidId_ReturnsCategoryWithLinks()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            var category = CategoryEntity.Create("Test Category", ImageInfo.Create("http://example.com/image.jpg"), null);
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/categories/{category.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var categoryWithLinks = await response.Content.ReadFromJsonAsync<LinkedResourceDto<CategoryDetailDto>>();
            categoryWithLinks.ShouldNotBeNull();
            categoryWithLinks.Resource.Name.ShouldBe("Test Category");
        }

        [Fact]
        public async Task UpdateCategory_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var updatedCategory = new CategoryDto
            {
                Id = 1,
                Name = "Updated Category",
            };

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.PutAsJsonAsync("/api/categories/999", updatedCategory); // Different ID

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_WithoutPermission_ReturnsForbidden()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var newCategory = new CategoryDto { Name = "Unauthorized Category" };

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.CustomerEmail);
            var response = await _client.PostAsJsonAsync("/api/categories", newCategory);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
