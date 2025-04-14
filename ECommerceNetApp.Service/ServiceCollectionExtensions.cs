using ECommerceNetApp.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddECommerceServices(this IServiceCollection services)
        {
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            return services;
        }
    }
}
