using System.Reflection;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.Implementation;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddECommerceServices(this IServiceCollection services)
        {
            services.AddScoped<IDomainEventService, DomainEventService>();
            return services;
        }

        public static IServiceCollection AddDispatcher(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.AddScoped<IDispatcher, Dispatcher>();
            services.AddSingleton<InMemoryMessageQueue>();
            services.AddSingleton<IEventBus, InMemoryEventBus>();

            RegisterHandlers(services, assembly, typeof(ICommandHandler<>));
            RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
            RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));
            RegisterHandlers(services, assembly, typeof(INotificationHandler<>));

            return services;
        }

        private static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly assembly, Type interfaceType)
        {
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .Select(t => new
                {
                    Implementation = t,
                    Interfaces = t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType),
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
