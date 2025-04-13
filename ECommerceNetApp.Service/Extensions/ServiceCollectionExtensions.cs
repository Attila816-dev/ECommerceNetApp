using ECommerceNetApp.Service.Implementation;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Mapping;
using ECommerceNetApp.Service.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddECommerceServices(this IServiceCollection services)
        {
            services.AddScoped<ICartItemMapper, CartItemMapper>();
            services.AddScoped<ICartService, CartService>();
            return services;
        }
    }
}
