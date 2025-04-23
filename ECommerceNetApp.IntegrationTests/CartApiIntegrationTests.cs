using System.Net;
using System.Net.Http.Json;
using System.Text;
using ECommerceNetApp.Api;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Persistence.Implementation.Cart;
using ECommerceNetApp.Service.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
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
                    services.Configure<CartDbOptions>(o => o.SeedSampleData = false);
                    services.Configure<ProductCatalogDbOptions>(o => o.SeedSampleData = false);

                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(CartDbContext));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Register the test CartDbContext with the test connection string
                    services.AddScoped(provider => new CartDbContext($"Filename={testDbPath};Mode=Shared"));
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
                var cartDbContext = scope.ServiceProvider.GetService<CartDbContext>();
                cartDbContext!.CreateCollection<CartEntity>();
                await cartDbContext.GetCollection<CartEntity>().InsertAsync(CartEntity.Create(cartId));
            }

            // Act
            var response = await _client.GetAsync($"/api/carts/{cartId}/items");

            // Assert
            response.EnsureSuccessStatusCode();
            var cartItems = await response.Content.ReadFromJsonAsync<List<CartItemDto>>();

            cartItems.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetCartItems_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var cartId = "invalid-cart-id-1";
            var response = await _client.GetAsync($"/api/carts/{cartId}/items");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AfterAddingItemToCart_ItemCanBeSuccessfullyQueried()
        {
            // Arrange
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
            var response = await _client.PostAsync($"/api/carts/{cartId}/items", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var itemsResponse = await _client.GetAsync($"/api/carts/{cartId}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<CartItemDto>>();

            items.ShouldNotBeNull();
            items.Count.ShouldBe(1);
            items[0].Name.ShouldBe("Test Product");
            items[0].Price.ShouldBe(19.99m);
            items[0].Quantity.ShouldBe(2);
        }

        [Fact]
        public async Task UpdateCartItem_UpdatesQuantitySuccessfully()
        {
            // Arrange
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

            await _client.PostAsync($"/api/carts/{cartId}/items", addContent);

            var updateQuantity = 5;
            using var updateContent = new StringContent(
                JsonConvert.SerializeObject(updateQuantity),
                Encoding.UTF8,
                "application/json");

            // Act
            var updateResponse = await _client.PutAsync($"/api/carts/{cartId}/items/1", updateContent);

            // Assert
            updateResponse.EnsureSuccessStatusCode();
            var itemsResponse = await _client.GetAsync($"/api/carts/{cartId}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<CartItemDto>>();

            items.ShouldNotBeNull();
            items.Count.ShouldBe(1);
            items[0].Quantity.ShouldBe(updateQuantity);
        }

        [Fact]
        public async Task RemoveCartItem_RemovesItemSuccessfully()
        {
            // Arrange
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

            await _client.PostAsync($"/api/carts/{cartId}/items", addContent);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/carts/{cartId}/items/1");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();
            var itemsResponse = await _client.GetAsync($"/api/carts/{cartId}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<CartItemDto>>();

            items.ShouldNotBeNull();
            items!.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetCartTotal_ReturnsCorrectTotal()
        {
            // Arrange
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

            await _client.PostAsync($"/api/carts/{cartId}/items", addContent);

            // Act
            var totalResponse = await _client.GetAsync($"/api/carts/{cartId}/total");

            // Assert
            totalResponse.EnsureSuccessStatusCode();
            var total = await totalResponse.Content.ReadFromJsonAsync<decimal>();

            total.ShouldBe(39.98m);
        }
    }
}