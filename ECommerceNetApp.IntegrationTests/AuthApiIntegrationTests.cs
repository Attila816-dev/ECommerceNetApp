using System.Net;
using System.Net.Http.Json;
using ECommerceNetApp.Api;
using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Implementation.Cart;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using ECommerceNetApp.Service.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ECommerceNetApp.IntegrationTests
{
    public class AuthApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthApiIntegrationTests(WebApplicationFactory<Program> factory)
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
        public async Task RegisterCustomer_WithValidData_ReturnsCreated()
        {
            // Arrange
            var newCustomer = new RegisterCustomerRequest
            {
                Email = "newcustomer@test.com",
                Password = "Customer123!",
                FirstName = "John",
                LastName = "Doe",
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/register/customer", newCustomer);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == newCustomer.Email);
            user.ShouldNotBeNull();
            user.Role.ShouldBe(UserRole.Customer);
        }

        [Fact]
        public async Task RegisterCustomer_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var invalidCustomer = new RegisterCustomerRequest
            {
                Email = "invalid-email", // Invalid email format
                Password = "123", // Too short password
                FirstName = string.Empty,
                LastName = string.Empty,
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/register/customer", invalidCustomer);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            var invalidLogin = new LoginUserCommand("nonexistent@test.com", "wrongpassword");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", invalidLogin);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            // First login to get refresh token
            var loginToken = await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.CustomerEmail);
            var loginRequest = new LoginUserCommand(ProductApiIntegrationTestsHelpers.CustomerEmail, ProductApiIntegrationTestsHelpers.GetPassword(ProductApiIntegrationTestsHelpers.CustomerEmail));
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserCommandResponse>();

            var refreshRequest = new RefreshTokenCommand(loginResult!.RefreshToken!);

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/refreshtoken", refreshRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var refreshResult = await response.Content.ReadFromJsonAsync<RefreshTokenCommandResponse>();
            refreshResult.ShouldNotBeNull();
            refreshResult.Success.ShouldBeTrue();
            refreshResult.AccessToken.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetCurrentUser_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/auth/me");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCurrentUser_WithValidToken_ReturnsUserInfo()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.CustomerEmail);
            var response = await _client.GetAsync("/api/v1/auth/me");

            // Assert
            response.EnsureSuccessStatusCode();
            var userWithLinks = await response.Content.ReadFromJsonAsync<LinkedResourceDto<UserDto>>();
            userWithLinks.ShouldNotBeNull();
            userWithLinks.Resource.Email.ShouldBe(ProductApiIntegrationTestsHelpers.CustomerEmail);
        }

        [Fact]
        public async Task GetUserByEmail_WithoutPermission_ReturnsForbidden()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await ProductApiIntegrationTestsHelpers.AddUsersAsync(scope.ServiceProvider);

            // Act
            await ProductApiIntegrationTestsHelpers.LoginAndSetTokenAsync(_client, ProductApiIntegrationTestsHelpers.CustomerEmail);
            var response = await _client.GetAsync($"/api/v1/auth/{ProductApiIntegrationTestsHelpers.AdminEmail}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}