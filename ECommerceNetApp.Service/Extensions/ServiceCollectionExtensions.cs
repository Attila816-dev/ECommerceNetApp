using System.Reflection;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Service.Implementation;
using ECommerceNetApp.Service.Implementation.EventBus;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDispatcher(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.AddScoped<IDispatcher, Dispatcher>();

            RegisterHandlers(services, assembly, typeof(ICommandHandler<>));
            RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
            RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));
            RegisterHandlers(services, assembly, typeof(INotificationHandler<>));

            return services;
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services, EventBusOptions eventBusOptions)
        {
            ArgumentNullException.ThrowIfNull(eventBusOptions, nameof(eventBusOptions));

            // Register the appropriate event bus implementation
            if (eventBusOptions.UseAzure)
            {
                if (string.IsNullOrWhiteSpace(eventBusOptions.AzureOptions?.ConnectionString))
                {
                    throw new InvalidOperationException("Azure Service Bus connection string must be provided when using Azure Event Bus");
                }

                services.AddSingleton<AzureServiceBusFactory>();
                services.AddSingleton<IEventBus, AzureEventBus>();
            }
            else if (eventBusOptions.UseAws)
            {
                services.AddSingleton<AWSEventBusFactory>();
                services.AddSingleton<IEventBus, AwsEventBus>();
            }
            else
            {
                services.AddSingleton<InMemoryMessageQueue>();
                services.AddSingleton<IEventBus, InMemoryEventBus>();
            }

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
