using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.Implementation;
using ECommerceNetApp.Service.Implementation.Mappers;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddECommerceServices(this IServiceCollection services)
        {
            services.AddScoped<ICartItemMapper, CartItemMapper>();
            services.AddScoped<IDomainEventService, DomainEventService>();
            return services;
        }
    }
}
