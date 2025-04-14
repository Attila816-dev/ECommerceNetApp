using System.Net.Http.Json;
using ECommerceNetApp.Domain;
using ECommerceNetApp.Persistence;
using ECommerceNetApp.Persistence.Implementation;
using ECommerceNetApp.Service;
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

            var category = new Category
            {
                Name = "Sample Category",
            };

            await dbContext.Categories.AddAsync(category);

            await dbContext.SaveChangesAsync();

            await dbContext.Products.AddAsync(new Product
            {
                Name = "Sample Product",
                Price = 10.99m,
                Amount = 5,
                CategoryId = category.Id,
            });

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

            var category = new Category
            {
                Name = "Sample Category",
            };

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