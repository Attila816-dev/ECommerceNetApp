using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    public class ProductCatalogDbSampleDataSeeder(
        ProductCatalogDbContext dbContext,
        IOptions<ProductCatalogDbOptions> productCatalogDbOptions,
        IPasswordService passwordService)
    {
        private readonly ProductCatalogDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly ProductCatalogDbOptions _productCatalogDbOptions = productCatalogDbOptions?.Value ?? throw new ArgumentNullException(nameof(productCatalogDbOptions));
        private readonly IPasswordService _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));

        public async Task SeedSampleDataAsync(CancellationToken cancellationToken = default)
        {
            if (!_productCatalogDbOptions.SeedSampleData)
            {
                return; // Skip seeding if disabled
            }

            if (!await _dbContext.Categories.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                await _dbContext.Categories.AddRangeAsync(
                    CategoryEntity.Create("Electronics", null, null),
                    CategoryEntity.Create("Books", null, null),
                    CategoryEntity.Create("Clothing", null, null))
                    .ConfigureAwait(false);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task SeedUsersAsync(CancellationToken cancellationToken)
        {
            if (!await _dbContext.Users.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                await _dbContext.Users.AddRangeAsync(GetPreconfiguredUsers(), cancellationToken).ConfigureAwait(false);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private List<UserEntity> GetPreconfiguredUsers()
        {
            return new List<UserEntity>
            {
                UserEntity.Create(
                    "admin@testdomain.com",
                    _passwordService.HashPassword("Admin123!"), // In production, use more secure passwords
                    "Attila",
                    "Németh",
                    UserRole.Admin),
                UserEntity.Create(
                    "ross@testdomain.com",
                    _passwordService.HashPassword("Customer123!"),
                    "Yadon",
                    "Ross",
                    UserRole.Customer),
                UserEntity.Create(
                    "dave@testdomain.com",
                    _passwordService.HashPassword("Jane123!"),
                    "Dave",
                    "Olsson",
                    UserRole.ProductManager),
            };
        }
    }
}
