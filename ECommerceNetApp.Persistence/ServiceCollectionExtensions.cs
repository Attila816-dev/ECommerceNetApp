using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Persistence
{
    public static class ServiceCollectionExtensions
    {
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

            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<CartDbInitializer>();
            services.AddScoped<CartDbSampleDataSeeder>();
            return services;
        }
    }
}
