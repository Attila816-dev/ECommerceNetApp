using ECommerceNetApp.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Persistence
{
    public static class ServiceCollectionExtensions
    {
        internal const string ProductCatalogDbConnectionStringName = "ProductCatalogDb";
        private const string CartDbConnectionStringName = "CartDb";

        public static IServiceCollection AddECommerceRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var cartDbConnectionString = configuration.GetConnectionString(CartDbConnectionStringName);
            ArgumentNullException.ThrowIfNull(cartDbConnectionString);

            services.AddSingleton(provider =>
            {
                return new CartDbContext(cartDbConnectionString);
            });

            services.AddDbContext<ProductCatalogDbContext>(options
                => options.UseSqlServer(
                    configuration.GetConnectionString(ProductCatalogDbConnectionStringName),
                    b => b.MigrationsAssembly(typeof(ProductCatalogDbContext).Assembly)));

            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<CartDbInitializer>();
            services.AddScoped<CartDbSampleDataSeeder>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }
    }
}
