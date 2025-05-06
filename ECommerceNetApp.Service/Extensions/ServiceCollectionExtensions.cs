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
            RegisterCommandHandlers(services, assembly);
            RegisterQueryHandlers(services, assembly);

            return services;
        }

        private static void RegisterCommandHandlers(IServiceCollection services, Assembly assembly)
        {
            var commandHandlerTypesWithResultTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
                .ToList();

            foreach (var commandHandlerType in commandHandlerTypesWithResultTypes)
            {
                var commandHandlerInterfaceType = commandHandlerType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));
                var commandType = commandHandlerInterfaceType.GetGenericArguments()[0];
                var resultType = commandHandlerInterfaceType.GetGenericArguments()[1];

                // Handle nullable result types
                var serviceType = typeof(ICommandHandler<,>).MakeGenericType(commandType, resultType);
                services.AddScoped(serviceType, commandHandlerType);

                if (Nullable.GetUnderlyingType(resultType) != null)
                {
                    var nullableServiceType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(Nullable<>).MakeGenericType(resultType));
                    services.AddScoped(nullableServiceType, commandHandlerType);
                }
            }

            var commandHandlerTypesWithoutResultTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
                .ToList();

            foreach (var commandHandlerType in commandHandlerTypesWithoutResultTypes)
            {
                var commandType = commandHandlerType.GetInterfaces()
                    .First(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
                    .GetGenericArguments()[0];
                var serviceType = typeof(ICommandHandler<>).MakeGenericType(commandType);
                services.AddScoped(serviceType, commandHandlerType);
            }
        }

        private static void RegisterQueryHandlers(IServiceCollection services, Assembly assembly)
        {
            var queryHandlerTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
                .ToList();

            foreach (var queryHandlerType in queryHandlerTypes)
            {
                var queryHandlerInterface = queryHandlerType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
                var queryType = queryHandlerInterface.GetGenericArguments()[0];
                var resultType = queryHandlerInterface.GetGenericArguments()[1];

                // Register the non-nullable version
                var serviceType = typeof(IQueryHandler<,>).MakeGenericType(queryType, resultType);
                services.AddScoped(serviceType, queryHandlerType);

                // Handle nullable reference types and nullable value types
                if (resultType.IsValueType && Nullable.GetUnderlyingType(resultType) != null)
                {
                    // Nullable value type (e.g., decimal?)
                    var nullableServiceType = typeof(IQueryHandler<,>).MakeGenericType(queryType, resultType);
                    services.AddScoped(nullableServiceType, queryHandlerType);
                }
                else if (!resultType.IsValueType && resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    // Handle IEnumerable<T> where T might be nullable
                    var elementType = resultType.GetGenericArguments()[0];
                    if (Nullable.GetUnderlyingType(elementType) != null || !elementType.IsValueType)
                    {
                        var nullableServiceType = typeof(IQueryHandler<,>).MakeGenericType(queryType, resultType);
                        services.AddScoped(nullableServiceType, queryHandlerType);
                    }
                }
            }
        }
    }
}
