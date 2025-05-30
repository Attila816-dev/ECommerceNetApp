using ECommerceNetApp.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ECommerceNetApp.Persistence.Implementation.ProductCatalog
{
    internal class ProductCatalogDbContextFactory : IDesignTimeDbContextFactory<ProductCatalogDbContext>
    {
        public ProductCatalogDbContext CreateDbContext(string[] args)
        {
            // Find the API project directory path
            // This assumes your Infrastructure project is at the same level as the API project
            var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ECommerceNetApp.Api");

            // Ensure the API directory exists
            if (!Directory.Exists(apiProjectPath))
            {
                throw new DirectoryNotFoundException(
                    $"Could not find API project directory at '{apiProjectPath}'. " +
                    "Make sure the path is correct relative to the Infrastructure project.");
            }

            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();

            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString(ServiceCollectionExtensions.ProductCatalogDbConnectionStringName);

            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<ProductCatalogDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ProductCatalogDbContext(optionsBuilder.Options, null, null);
        }
    }
}
