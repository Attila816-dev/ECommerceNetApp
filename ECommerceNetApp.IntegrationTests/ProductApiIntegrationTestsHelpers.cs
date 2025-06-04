using System.Net.Http.Headers;
using System.Net.Http.Json;
using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.User;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ECommerceNetApp.IntegrationTests
{
    internal static class ProductApiIntegrationTestsHelpers
    {
        public const string AdminEmail = "testadmin@testdomain.com";
        public const string CustomerEmail = "testcustomer@testdomain.com";
        public const string ManagerEmail = "testmanager@testdomain.com";

        private static readonly Dictionary<string, string> UsersWithPasswordDictionary = new Dictionary<string, string>();

        internal static string GetPassword(string email)
        {
            ArgumentException.ThrowIfNullOrEmpty(email);
            if (UsersWithPasswordDictionary.TryGetValue(email, out string? password))
            {
                return password;
            }

            throw new InvalidOperationException($"Invalid email: {email}");
        }

        internal static async Task AddUsersAsync(IServiceProvider serviceProvider)
        {
            var passwordService = serviceProvider.GetRequiredService<IPasswordService>();
            var dbContext = serviceProvider.GetRequiredService<ProductCatalogDbContext>();

            UsersWithPasswordDictionary.Clear();
            UsersWithPasswordDictionary[AdminEmail] = "Admin123!"; // In production, use more secure passwords
            UsersWithPasswordDictionary[CustomerEmail] = "Customer123!";
            UsersWithPasswordDictionary[ManagerEmail] = "Jane123!";

            var users = new List<UserEntity>
            {
                UserEntity.Create(
                    AdminEmail,
                    passwordService.HashPassword(UsersWithPasswordDictionary[AdminEmail]),
                    "Attila",
                    "Németh",
                    UserRole.Admin),
                UserEntity.Create(
                    CustomerEmail,
                    passwordService.HashPassword(UsersWithPasswordDictionary[CustomerEmail]),
                    "Yadon",
                    "Ross",
                    UserRole.Customer),
                UserEntity.Create(
                    ManagerEmail,
                    passwordService.HashPassword(UsersWithPasswordDictionary[ManagerEmail]),
                    "Dave",
                    "Olsson",
                    UserRole.ProductManager),
            };

            await dbContext.Users.AddRangeAsync(users, CancellationToken.None).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        }

        internal static async Task<string> LoginAndSetTokenAsync(HttpClient client, string email)
        {
            // Clear any existing authorization headers
            client.DefaultRequestHeaders.Authorization = null;

            var loginRequest = new LoginUserCommand(email, GetPassword(email));
            using var loginContent = JsonContent.Create(loginRequest);
            var loginResponse = await client.PostAsync("/api/v1/auth/login", loginContent);

            loginResponse.EnsureSuccessStatusCode();

            var loginCommandResponse = await loginResponse.Content.ReadFromJsonAsync<LoginUserCommandResponse>();

            loginCommandResponse.ShouldNotBeNull();
            loginCommandResponse.AccessToken.ShouldNotBeNull();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginCommandResponse.AccessToken);
            return loginCommandResponse.AccessToken;
        }
    }
}