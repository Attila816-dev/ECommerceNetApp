using System.Reflection;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.Implementation;
using ECommerceNetApp.Service.Implementation.Mappers.Category;
using ECommerceNetApp.Service.Implementation.Mappers.Product;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Interfaces.Mappers.Category;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddECommerceServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryMapper, CategoryMapper>();
            services.AddScoped<IProductMapper, ProductMapper>();
            services.AddScoped<IDomainEventService, DomainEventService>();
            return services;
        }

        public static IServiceCollection AddDispatcher(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.AddScoped<IDispatcher, Dispatcher>();
            services.AddScoped<IPublisher, SimplePublisher>();
            RegisterHandlers(services, assembly);

            return services;
        }

        public static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .Select(t => new
                {
                    Implementation = t,
                    Interfaces = t.GetInterfaces()
                        .Where(i => i.IsGenericType && (
                            i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                            i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                            i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))),
                })
                .Where(t => t.Interfaces.Any());

            foreach (var handler in handlerTypes)
            {
                foreach (var @interface in handler.Interfaces)
                {
                    services.AddTransient(@interface, handler.Implementation);
                }
            }

            return services;
        }
    }
}
