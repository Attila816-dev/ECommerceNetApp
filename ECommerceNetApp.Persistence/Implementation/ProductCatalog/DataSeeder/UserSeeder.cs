using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    public class UserSeeder
    {
        private static readonly Action<ILogger, Exception?> _LogUserSeeding = LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(_LogUserSeeding)), "Seeding users...");

        private static readonly Action<ILogger, Exception?> _LogUserSeededSuccessfully = LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(_LogUserSeededSuccessfully)), "Users seeded successfully");

        private static readonly Action<ILogger, Exception?> _LogUserSeedingError = LoggerMessage.Define(LogLevel.Error, new EventId(3, nameof(_LogUserSeedingError)), "An error occurred while seeding the database");

        private readonly ProductCatalogDbContext _dbContext;
        private readonly ILogger<UserSeeder> _logger;
        private readonly IOptions<ProductCatalogDbOptions> _options;
        private readonly IPasswordService _passwordService;

        public UserSeeder(
            ProductCatalogDbContext dbContext,
            ILogger<UserSeeder> logger,
            IOptions<ProductCatalogDbOptions> options,
            IPasswordService passwordService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _options = options;
            _passwordService = passwordService;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_options.Value.SeedSampleData)
                {
                    return; // Skip seeding if disabled
                }

                // Only seed if database is empty
                bool hasAnyUsers = await _dbContext.Users.AnyAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!hasAnyUsers)
                {
                    _LogUserSeeding.Invoke(_logger, null);
                    await SeedUsersAsync(cancellationToken).ConfigureAwait(false);
                    _LogUserSeededSuccessfully.Invoke(_logger, null);
                }
            }
            catch (Exception ex)
            {
                _LogUserSeedingError.Invoke(_logger, ex);
                throw;
            }
        }

        private async Task SeedUsersAsync(CancellationToken cancellationToken)
        {
            var users = new List<UserEntity>
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

            await _dbContext.Users.AddRangeAsync(users, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
