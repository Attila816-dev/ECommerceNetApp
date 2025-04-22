using System.Net.Http.Json;
using ECommerceNetApp.Api;
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
                    services.Remove(services.Single(d => d.ServiceType == typeof(CartDbContext)));
                    services.AddSingleton(new CartDbContext("Filename=:memory:;Mode=Memory;Cache=Shared"));

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
        public async Task GetAllProducts_ReturnsProducts()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            var category = new CategoryEntity("Sample Category");

            await dbContext.Categories.AddAsync(category);

            await dbContext.SaveChangesAsync();

            var product = new ProductEntity("Sample Product", null, null, category, new Money(10.99m, null), 5);

            await dbContext.Products.AddAsync(product);

            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            products.ShouldNotBeNull();
            products.ShouldContain(p => p.Name.Equals("Sample Product", StringComparison.Ordinal) && p.Amount == 5);
        }

        [Fact]
        public async Task CreateProduct_AddsProductSuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            var category = new CategoryEntity("Sample Category");

            await dbContext.Categories.AddAsync(category);

            await dbContext.SaveChangesAsync();

            var newProduct = new ProductDto
            {
                Name = "New Product",
                Price = 19.99m,
                Amount = 10,
                CategoryId = category.Id,
            };

            using var content = JsonContent.Create(newProduct);

            // Act
            var response = await _client.PostAsync("/api/products", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var product = await dbContext.Products.FirstAsync(p => p.Name == "New Product" && p.CategoryId == category.Id);
            product.ShouldNotBeNull();
            product.Amount.ShouldBe(10);
        }
    }
}