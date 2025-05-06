using ECommerceNetApp.Persistence.Implementation.Cart;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder;

namespace ECommerceNetApp.Api.Services
{
    public class DatabaseInitializer : IHostedService
    {
        private static readonly Action<ILogger, Exception?> LogInitializingDatabases =
            LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(LogInitializingDatabases)), "Initializing databases...");

        private static readonly Action<ILogger, Exception?> LogDatabasesInitializedSuccessfully =
            LoggerMessage.Define(LogLevel.Information, new EventId(2, nameof(LogDatabasesInitializedSuccessfully)), "Databases initialized successfully");

        private static readonly Action<ILogger, Exception?> LogErrorInitializingDatabases =
            LoggerMessage.Define(LogLevel.Error, new EventId(3, nameof(LogErrorInitializingDatabases)), "An error occurred while initializing the databases");

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            IServiceProvider serviceProvider,
            ILogger<DatabaseInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            LogInitializingDatabases(_logger, null);

            using var scope = _serviceProvider.CreateScope();

            try
            {
                // Initialize product catalog database
                var productCatalogMigrator = scope.ServiceProvider.GetRequiredService<ProductCatalogDbMigrator>();
                await productCatalogMigrator.MigrateDatabaseAsync(cancellationToken).ConfigureAwait(false);

                var productCatalogSeeder = scope.ServiceProvider.GetRequiredService<ProductCatalogDataSeeder>();
                await productCatalogSeeder.SeedAsync(cancellationToken).ConfigureAwait(false);

                // Initialize cart database
                var cartInitializer = scope.ServiceProvider.GetRequiredService<CartDbInitializer>();
                await cartInitializer.InitializeDatabaseAsync(cancellationToken).ConfigureAwait(false);

                var cartSeeder = scope.ServiceProvider.GetRequiredService<CartSeeder>();
                await cartSeeder.SeedAsync(cancellationToken).ConfigureAwait(false);

                LogDatabasesInitializedSuccessfully(_logger, null);
            }
            catch (Exception ex)
            {
                LogErrorInitializingDatabases(_logger, ex);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
