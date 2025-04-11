using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Persistence
{
    public static class ServiceCollectionExtensions
    {
        private const string CartDbConnectionStringName = "CartDb";

        public static IServiceCollection AddECommerceRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            var cartDbConnectionString = configuration.GetConnectionString(CartDbConnectionStringName) ?? throw new ArgumentNullException(CartDbConnectionStringName);
            services.AddSingleton(new CartDbContext(cartDbConnectionString));
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<CartDbInitializer>();
            services.AddScoped<CartDbSampleDataSeeder>();
            return services;
        }
    }
}
