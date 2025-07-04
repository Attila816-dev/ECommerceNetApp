﻿using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;
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

        private readonly ProductCatalogDbContext _dbContext;
        private readonly IEnumerable<IProductDataSeeder> _productSeeders;
        private readonly IEnumerable<ICategoryDataSeeder> _categorySeeders;
        private readonly ILogger<ProductCatalogDataSeeder> _logger;
        private readonly IOptions<ProductCatalogDbOptions> _options;

        public ProductCatalogDataSeeder(
            ProductCatalogDbContext dbContext,
            IEnumerable<IProductDataSeeder> productSeeders,
            IEnumerable<ICategoryDataSeeder> categorySeeders,
            ILogger<ProductCatalogDataSeeder> logger,
            IOptions<ProductCatalogDbOptions> options)
        {
            _dbContext = dbContext;
            _productSeeders = productSeeders;
            _categorySeeders = categorySeeders;
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

                // Only seed if database is empty
                bool hasAnyCategories = await _dbContext.Categories.AnyAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                bool hasAnyProducts = await _dbContext.Products.AnyAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!hasAnyCategories && !hasAnyProducts)
                {
                    _LogProductCatalogSeeding.Invoke(_logger, null);
                    await SeedProductCatalogAsync(cancellationToken).ConfigureAwait(false);
                    _LogProductCatalogSeededSuccessfully.Invoke(_logger, null);
                }
            }
            catch (Exception ex)
            {
                _LogProductCatalogSeedingError.Invoke(_logger, ex);
                throw;
            }
        }

        private async Task SeedProductCatalogAsync(CancellationToken cancellationToken)
        {
            // Main departments
            var groceriesCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Root.GroceriesCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}groceries.jpg"),
                null);

            await _dbContext.Categories.AddAsync(groceriesCategory, cancellationToken).ConfigureAwait(false);

            var householdCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Root.HouseholdCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}household.jpg"),
                null);

            await _dbContext.Categories.AddAsync(householdCategory, cancellationToken).ConfigureAwait(false);

            var electronicsCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Root.ElectronicsCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}electronics.jpg"),
                null);

            await _dbContext.Categories.AddAsync(electronicsCategory, cancellationToken).ConfigureAwait(false);

            var clothingCategory = CategoryEntity.Create(
                ProductCatalogConstants.CategoryNames.Root.ClothingCategoryName,
                ImageInfo.Create($"{ProductCatalogConstants.ImagePrefix.Category}clothing.jpg"),
                null);

            await _dbContext.Categories.AddAsync(clothingCategory, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            foreach (var categorySeeder in _categorySeeders)
            {
                await categorySeeder.SeedCategoriesAsync(cancellationToken).ConfigureAwait(false);
            }

            foreach (var productSeeder in _productSeeders)
            {
                await productSeeder.SeedProductsAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
