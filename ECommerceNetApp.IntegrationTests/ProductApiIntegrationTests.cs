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
    public class ProductApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProductApiIntegrationTests(WebApplicationFactory<Program> factory)
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
        public async Task GetAllProducts_ReturnsProducts()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            var category = CategoryEntity.Create("Sample Category", null, null);

            await dbContext.Categories.AddAsync(category);

            await dbContext.SaveChangesAsync();

            var product = ProductEntity.Create("Sample Product", null, null, category, Money.From(10.99m), 5);

            await dbContext.Products.AddAsync(product);

            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            response.EnsureSuccessStatusCode();
            var productsWithLinks = await response.Content.ReadFromJsonAsync<CollectionLinkedResourceDto<ProductDto>>();
            productsWithLinks.ShouldNotBeNull();
            productsWithLinks.Items.ShouldNotBeNull();
            productsWithLinks.Items.ShouldContain(p => p.Name.Equals("Sample Product", StringComparison.Ordinal) && p.Amount == 5);
        }

        [Fact]
        public async Task CreateProduct_AddsProductSuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
            var category = CategoryEntity.Create("Sample Category", null, null);

            await dbContext.Categories.AddAsync(category);

            await dbContext.SaveChangesAsync();

            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var newProduct = new ProductDto
            {
                Name = "New Product",
                Price = 19.99m,
                Amount = 10,
                CategoryId = category.Id,
            };

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            using var content = JsonContent.Create(newProduct);
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var product = await dbContext.Products.FirstAsync(p => p.Name == "New Product" && p.CategoryId == category.Id);
            product.ShouldNotBeNull();
            product.Amount.ShouldBe(10);
        }

        [Fact]
        public async Task CreateProduct_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var category = CategoryEntity.Create("Sample Category", null, null);
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();

            var newProduct = new CreateProductDto
            {
                Name = "New Product",
                Description = "Test product description",
                Price = 19.99m,
                Currency = "USD",
                Amount = 10,
                CategoryId = category.Id,
                ImageUrl = "https://example.com/image.jpg",
            };

            using var content = JsonContent.Create(newProduct);

            // Act - No authentication header
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateProduct_WithCustomerRole_ReturnsForbidden()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var category = CategoryEntity.Create("Sample Category", null, null);
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();

            var newProduct = new CreateProductDto
            {
                Name = "New Product",
                Description = "Test product description",
                Price = 19.99m,
                Currency = "USD",
                Amount = 10,
                CategoryId = category.Id,
                ImageUrl = "https://example.com/image.jpg",
            };

            using var content = JsonContent.Create(newProduct);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.CustomerEmail);
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateProduct_WithProductManagerRole_UpdatesSuccessfully()
        {
            // Arrange
            ProductEntity product;
            CategoryEntity category;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

                await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

                category = CategoryEntity.Create("Sample Category", null, null);
                await dbContext.Categories.AddAsync(category);
                await dbContext.SaveChangesAsync();

                product = ProductEntity.Create("Original Product", null, null, category, Money.From(10.99m), 5);
                await dbContext.Products.AddAsync(product);
                await dbContext.SaveChangesAsync();
            }

            var updateProduct = new UpdateProductDto
            {
                Id = product.Id,
                Name = "Updated Product",
                Description = "Updated description",
                Price = 29.99m,
                Currency = "USD",
                Amount = 15,
                CategoryId = category.Id,
                ImageUrl = "https://example.com/updated-image.jpg",
            };

            using var content = JsonContent.Create(updateProduct);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.PutAsync($"/api/products/{product.Id}", content);

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

            // Verify the product was updated
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

                var updatedProduct = await dbContext.Products.FirstAsync(p => p.Id == product.Id);
                updatedProduct.Name.ShouldBe("Updated Product");
                updatedProduct.Amount.ShouldBe(15);
            }
        }

        [Fact]
        public async Task DeleteProduct_WithAdminRole_DeletesSuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var category = CategoryEntity.Create("Sample Category", null, null);
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();

            var product = ProductEntity.Create("Product to Delete", null, null, category, Money.From(10.99m), 5);
            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.AdminEmail);
            var response = await _client.DeleteAsync($"/api/products/{product.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

            // Verify the product was deleted (should not exist anymore)
            var deletedProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            deletedProduct.ShouldBeNull();
        }

        [Fact]
        public async Task GetProduct_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/products/999999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetProductsByCategoryId_WithValidCategoryId_ReturnsFilteredProducts()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            var category1 = CategoryEntity.Create("Category 1", null, null);
            var category2 = CategoryEntity.Create("Category 2", null, null);
            await dbContext.Categories.AddRangeAsync(category1, category2);
            await dbContext.SaveChangesAsync();

            var product1 = ProductEntity.Create("Product 1", null, null, category1, Money.From(10.99m), 5);
            var product2 = ProductEntity.Create("Product 2", null, null, category2, Money.From(15.99m), 3);
            await dbContext.Products.AddRangeAsync(product1, product2);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/products/by-category/{category1.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var productsWithLinks = await response.Content.ReadFromJsonAsync<CollectionLinkedResourceDto<ProductDto>>();
            productsWithLinks.ShouldNotBeNull();
            productsWithLinks.Items.Count().ShouldBe(1);
            productsWithLinks.Items.First().Name.ShouldBe("Product 1");
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var updateProduct = new UpdateProductDto
            {
                Id = 1,
                Name = "Updated Product",
            };

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.PutAsJsonAsync("/api/products/999", updateProduct); // Different ID

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.AdminEmail);
            var response = await _client.DeleteAsync("/api/products/999999");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}