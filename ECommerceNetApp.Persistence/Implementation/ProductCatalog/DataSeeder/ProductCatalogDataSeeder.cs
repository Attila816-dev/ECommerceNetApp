using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder
{
    public class ProductCatalogDataSeeder
    {
        private static readonly Action<ILogger, Exception?> _LogProductCatalogSeeding =
            LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(_LogProductCatalogSeeding)), "Seeding product catalog database...");

        private static readonly Action<ILogger, Exception?> _LogProductCatalogSeededSuccessfully =
            LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(_LogProductCatalogSeededSuccessfully)), "Product catalog database seeded successfully");

        private static readonly Action<ILogger, Exception?> _LogProductCatalogSeedingError =
            LoggerMessage.Define(LogLevel.Error, new EventId(3, nameof(_LogProductCatalogSeedingError)), "An error occurred while seeding the database");

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProductCatalogDataSeeder> _logger;
        private readonly IOptions<ProductCatalogDbOptions> _options;

        public ProductCatalogDataSeeder(
            IServiceProvider serviceProvider,
            ILogger<ProductCatalogDataSeeder> logger,
            IOptions<ProductCatalogDbOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_options.Value.SeedSampleData)
                {
                    return; // Skip seeding if disabled
                }

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

                // Apply pending migrations
                await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

                // Only seed if database is empty
                bool hasAnyCategories = await dbContext.Categories.AnyAsync(cancellationToken).ConfigureAwait(false);
                bool hasAnyProducts = await dbContext.Products.AnyAsync(cancellationToken).ConfigureAwait(false);
                if (!hasAnyCategories && !hasAnyProducts)
                {
                    _LogProductCatalogSeeding.Invoke(_logger, null);
                    await SeedProductCatalogAsync(dbContext, scope.ServiceProvider, cancellationToken).ConfigureAwait(false);
                    _LogProductCatalogSeededSuccessfully.Invoke(_logger, null);
                }
            }
            catch (Exception ex)
            {
                _LogProductCatalogSeedingError.Invoke(_logger, ex);
                throw;
            }
        }

        private static async Task SeedProductCatalogAsync(ProductCatalogDbContext dbContext, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            // Main departments
            var groceriesCategory = CategoryEntity.Create(
                ProductCatalogConstants.GroceriesCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}groceries.jpg"),
                null);

            var householdCategory = CategoryEntity.Create(
                ProductCatalogConstants.HouseholdCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}household.jpg"),
                null);

            var electronicsCategory = CategoryEntity.Create(
                ProductCatalogConstants.ElectronicsCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}electronics.jpg"),
                null);

            var clothingCategory = CategoryEntity.Create(
                ProductCatalogConstants.ClothingCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.CategoryImagePrefix}clothing.jpg"),
                null);

            await dbContext.Categories.AddRangeAsync(groceriesCategory, householdCategory, electronicsCategory, clothingCategory).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var categoriesSeeders = serviceProvider.GetServices<ICategoryDataSeeder>();
            foreach (var categoriesSeeder in categoriesSeeders)
            {
                await categoriesSeeder.SeedCategoriesAsync(cancellationToken).ConfigureAwait(false);
            }

            var productsSeeders = serviceProvider.GetServices<IProductDataSeeder>();
            foreach (var productsSeeder in productsSeeders)
            {
                await productsSeeder.SeedProductsAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
