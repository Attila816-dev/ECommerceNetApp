using ECommerceNetApp.Persistence.Implementation.Cart;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
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
            services.AddScoped<ProductCatalogDbSampleDataSeeder>();

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
            services.AddScoped<CartDbSampleDataSeeder>();
            services.AddScoped<ICartRepositoryFactory, CartRepositoryFactory>();
            services.AddScoped<ICartUnitOfWork, CartUnitOfWork>();
            return services;
        }
    }
}
