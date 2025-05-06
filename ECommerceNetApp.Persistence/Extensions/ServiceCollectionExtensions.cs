using System.Reflection;
using ECommerceNetApp.Persistence.Implementation.Cart;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog.DataSeeder;
using ECommerceNetApp.Persistence.Interfaces.Cart;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        internal const string ProductCatalogDbConnectionStringName = "ProductCatalogDb";
        private const string CartDbConnectionStringName = "CartDb";

        public static IServiceCollection AddProductCatalogDb(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddDbContext<ProductCatalogDbContext>(options
                => options.UseSqlServer(
                    configuration.GetConnectionString(ProductCatalogDbConnectionStringName),
                    b => b.MigrationsAssembly(typeof(ProductCatalogDbContext).Assembly)));

            services.AddScoped<IProductCatalogUnitOfWork, ProductCatalogUnitOfWork>();
            services.AddScoped<ProductCatalogDataSeeder>();
            services.AddScoped<ProductCatalogDbMigrator>();
            services.AddProductCatalogDataSeeders();

            return services;
        }

        public static IServiceCollection AddCartDb(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var cartDbConnectionString = configuration.GetConnectionString(CartDbConnectionStringName);
            ArgumentNullException.ThrowIfNull(cartDbConnectionString);

            services.AddScoped(provider =>
            {
                return new CartDbContext(cartDbConnectionString);
            });

            services.AddScoped<CartDbInitializer>();
            services.AddScoped<CartSeeder>();
            services.AddScoped<ICartRepositoryFactory, CartRepositoryFactory>();
            services.AddScoped<ICartUnitOfWork, CartUnitOfWork>();
            return services;
        }

        private static IServiceCollection AddProductCatalogDataSeeders(this IServiceCollection services)
        {
            // Get the assembly containing the IProductDataSeeder implementations
            var assembly = Assembly.GetAssembly(typeof(ProductCatalogDbContext));

            // Find all types implementing IProductDataSeeder
            var productSeederTypes = assembly!.GetTypes()
                .Where(type => typeof(IProductDataSeeder).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            // Register each implementation as a transient service
            foreach (var productSeeder in productSeederTypes)
            {
                services.AddTransient(typeof(IProductDataSeeder), productSeeder);
            }

            // Find all types implementing ICategoryDataSeeder
            var categorySeederTypes = assembly.GetTypes()
                .Where(type => typeof(ICategoryDataSeeder).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            // Register each implementation as a transient service
            foreach (var categorySeeder in categorySeederTypes)
            {
                services.AddTransient(typeof(ICategoryDataSeeder), categorySeeder);
            }

            return services;
        }
    }
}
