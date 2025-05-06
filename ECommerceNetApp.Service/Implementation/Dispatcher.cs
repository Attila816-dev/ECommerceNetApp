using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service.Implementation
{
    public class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public async Task<TResponse> SendQueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResponse>
        {
            ArgumentNullException.ThrowIfNull(query, nameof(query));
            var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResponse>>()
                ?? throw new InvalidOperationException($"Handler for query {query.GetType().Name} not found.");

            var behaviors = serviceProvider.GetServices<IPipelineBehavior<TQuery, TResponse>>().Reverse();

            RequestHandlerDelegate<TResponse> handlerDelegate = (ct) => handler.HandleAsync(query, ct);
            foreach (var behavior in behaviors)
            {
                var next = handlerDelegate;
                handlerDelegate = (ct) => behavior.HandleAsync(query, next, ct);
            }

            return await handlerDelegate(cancellationToken).ConfigureAwait(false);
        }

        public async Task SendCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>()
                ?? throw new InvalidOperationException($"Handler for command {command.GetType().Name} not found.");

            var behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand, bool>>().Reverse();

            RequestHandlerDelegate<bool> handlerDelegate = async (ct) =>
            {
                await handler.HandleAsync(command, ct).ConfigureAwait(false);
                return true;
            };

            foreach (var behavior in behaviors)
            {
                var next = handlerDelegate;
                handlerDelegate = (ct) => behavior.HandleAsync(command, next, ct);
            }

            await handlerDelegate(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TResponse> SendCommandAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResponse>
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResponse>>()
                ?? throw new InvalidOperationException($"Handler for command {command.GetType().Name} not found.");

            var behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand, TResponse>>().Reverse();

            RequestHandlerDelegate<TResponse> handlerDelegate = (ct) => handler.HandleAsync(command, ct);
            foreach (var behavior in behaviors)
            {
                var next = handlerDelegate;
                handlerDelegate = (ct) => behavior.HandleAsync(command, next, ct);
            }

            return await handlerDelegate(cancellationToken).ConfigureAwait(false);
        }
    }
}
