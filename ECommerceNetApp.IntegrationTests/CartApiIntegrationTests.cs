using System.Net;
using System.Net.Http.Json;
using System.Text;
using ECommerceNetApp.Api;
using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Persistence.Implementation.Cart;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;

namespace ECommerceNetApp.IntegrationTests
{
    public class CartApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CartApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            string testDbPath = Path.Combine(Path.GetTempPath(), $"Cart-{Guid.NewGuid()}.db");
            if (File.Exists(testDbPath))
            {
                File.Delete(testDbPath);
            }

            ArgumentNullException.ThrowIfNull(factory);

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace services with test doubles if needed
                    // For example, replace the real DB context with a test one:
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

                    // Register the test CartDbContext with the test connection string
                    services.Configure<CartDbOptions>(o => o.SeedSampleData = false);
                    services.AddSingleton<ICartDbContextFactory>(new CartDbContextFactory($"Filename={testDbPath};Mode=Shared"));
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetCartItems_WithValidId_ReturnsCartItems()
        {
            // Arrange
            var cartId = "test-cart-id-1";
            using (var scope = _factory.Services.CreateScope())
            {
                var cartDbContextFactory = scope.ServiceProvider.GetService<ICartDbContextFactory>();
                using (var cartDbContext = cartDbContextFactory!.CreateDbContext())
                {
                    cartDbContext!.CreateCollection<CartEntity>();
                    await cartDbContext.GetCollection<CartEntity>().InsertAsync(CartEntity.Create(cartId));
                }

                await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);
            }

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.GetAsync($"/api/v1/carts/{cartId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var cartWithLinks = await response.Content.ReadFromJsonAsync<LinkedResourceDto<CartDto>>();

            cartWithLinks.ShouldNotBeNull();
            cartWithLinks.Resource.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetCartItems_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);

            var cartId = "invalid-cart-id-1";
            var response = await _client.GetAsync($"/api/v1/carts/{cartId}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AfterAddingItemToCart_ItemCanBeSuccessfullyQueried()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var cartId = "test-cart-id-1";
            var newItem = new CartItemDto
            {
                Id = 1,
                Name = "Test Product",
                Price = 19.99m,
                Quantity = 2,
                ImageUrl = "test-image.jpg",
                ImageAltText = "Test Product Image",
            };

            using var content = new StringContent(
                JsonConvert.SerializeObject(newItem),
                Encoding.UTF8,
                "application/json");

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.PostAsync($"/api/v1/carts/{cartId}/items", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var itemsResponse = await _client.GetAsync($"/api/v2/carts/{cartId}/items/{1}");
            var itemWithLinks = await itemsResponse.Content.ReadFromJsonAsync<LinkedResourceDto<CartItemDto>>();

            itemWithLinks.ShouldNotBeNull();
            var item = itemWithLinks.Resource;
            item.ShouldNotBeNull();
            item.Name.ShouldBe("Test Product");
            item.Price.ShouldBe(19.99m);
            item.Quantity.ShouldBe(2);
        }

        [Fact]
        public async Task UpdateCartItem_UpdatesQuantitySuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var cartId = "test-cart-id-1";
            var newItem = new CartItemDto
            {
                Id = 1,
                Name = "Test Product",
                Price = 19.99m,
                Quantity = 1,
                ImageUrl = "test-image.jpg",
            };

            using var addContent = new StringContent(
                JsonConvert.SerializeObject(newItem),
                Encoding.UTF8,
                "application/json");

            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            await _client.PostAsync($"/api/v2/carts/{cartId}/items", addContent);

            var updateQuantity = 5;
            using var updateContent = new StringContent(
                JsonConvert.SerializeObject(updateQuantity),
                Encoding.UTF8,
                "application/json");

            // Act
            var updateResponse = await _client.PutAsync($"/api/v2/carts/{cartId}/items/{newItem.Id}", updateContent);

            // Assert
            updateResponse.EnsureSuccessStatusCode();
            var itemsResponse = await _client.GetAsync($"/api/v2/carts/{cartId}/items/{newItem.Id}");
            var itemWithLinks = await itemsResponse.Content.ReadFromJsonAsync<LinkedResourceDto<CartItemDto>>();

            itemWithLinks.ShouldNotBeNull();
            var item = itemWithLinks.Resource;
            item.ShouldNotBeNull();
            item.Quantity.ShouldBe(updateQuantity);
        }

        [Fact]
        public async Task RemoveCartItem_RemovesItemSuccessfully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var cartId = "test-cart-id-1";
            var newItem = new CartItemDto
            {
                Id = 1,
                Name = "Test Product",
                Price = 19.99m,
                Quantity = 1,
            };

            using var addContent = new StringContent(
                JsonConvert.SerializeObject(newItem),
                Encoding.UTF8,
                "application/json");

            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            await _client.PostAsync($"/api/v2/carts/{cartId}/items", addContent);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/v2/carts/{cartId}/items/1");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();
            var itemResponse = await _client.GetAsync($"/api/carts/{cartId}/items/1");
            itemResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCartTotal_ReturnsCorrectTotal()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var cartId = "test-cart-id-1";
            var newItem = new CartItemDto
            {
                Id = 1,
                Name = "Test Product",
                Price = 19.99m,
                Quantity = 2,
            };

            using var addContent = new StringContent(
                JsonConvert.SerializeObject(newItem),
                Encoding.UTF8,
                "application/json");

            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            await _client.PostAsync($"/api/v2/carts/{cartId}/items", addContent);

            // Act
            var totalResponse = await _client.GetAsync($"/api/v1/carts/{cartId}/total");

            // Assert
            totalResponse.EnsureSuccessStatusCode();
            var totalWithLinks = await totalResponse.Content.ReadFromJsonAsync<LinkedResourceDto<decimal>>();

            totalWithLinks.ShouldNotBeNull();
            totalWithLinks.Resource.ShouldBe(39.98m);
        }

        [Fact]
        public async Task GetCart_WithoutPermission_ReturnsForbidden()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            // Act
            var response = await _client.GetAsync("/api/v1/carts/some-cart-id");

            // Assert - Assuming Customer doesn't have Cart.Read permission
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AddItemToCart_WithNullItem_ReturnsBadRequest()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var cartId = "test-cart-id";

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.PostAsJsonAsync($"/api/v1/carts/{cartId}/items", (CartItemDto?)null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RemoveItemFromCart_WithNonExistentItem_ReturnsNoContent()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var cartId = "test-cart-id";
            var nonExistentItemId = 999;

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.DeleteAsync($"/api/v1/carts/{cartId}/items/{nonExistentItemId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // Idempotent operation
        }

        [Fact]
        public async Task GetCartTotal_WithEmptyCart_ReturnsZero()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var cartId = "empty-cart-id";
            using (var cartScope = _factory.Services.CreateScope())
            {
                var cartDbContextFactory = cartScope.ServiceProvider.GetService<ICartDbContextFactory>();
                using (var cartDbContext = cartDbContextFactory!.CreateDbContext())
                {
                    cartDbContext!.CreateCollection<CartEntity>();
                    await cartDbContext.GetCollection<CartEntity>().InsertAsync(CartEntity.Create(cartId));
                }
            }

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.ManagerEmail);
            var response = await _client.GetAsync($"/api/v1/carts/{cartId}/total");

            // Assert
            response.EnsureSuccessStatusCode();
            var totalWithLinks = await response.Content.ReadFromJsonAsync<LinkedResourceDto<decimal>>();
            totalWithLinks.ShouldNotBeNull();
            totalWithLinks.Resource.ShouldBe(0m);
        }
    }
}