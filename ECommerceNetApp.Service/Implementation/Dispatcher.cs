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
            ArgumentNullException.ThrowIfNull(query);

            RequestHandlerDelegate<TResponse> handlerDelegate = async (ct) =>
            {
                var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResponse>>()
                    ?? throw new InvalidOperationException($"Handler for query {query.GetType().Name} not found.");
                var result = await handler.HandleAsync(query, ct).ConfigureAwait(false);
                return result;
            };

            var behaviors = serviceProvider.GetServices<IPipelineBehavior<TQuery, TResponse>>().Reverse();
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
            ArgumentNullException.ThrowIfNull(command);

            RequestHandlerDelegate<bool> handlerDelegate = async (ct) =>
            {
                var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>()
                    ?? throw new InvalidOperationException($"Handler for command {command.GetType().Name} not found.");
                await handler.HandleAsync(command, ct).ConfigureAwait(false);
                return true;
            };

            var behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand, bool>>().Reverse();
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
            ArgumentNullException.ThrowIfNull(command);

            RequestHandlerDelegate<TResponse> handlerDelegate = async (ct) =>
            {
                var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResponse>>()
                    ?? throw new InvalidOperationException($"Handler for command {command.GetType().Name} not found.");
                var result = await handler.HandleAsync(command, ct).ConfigureAwait(false);
                return result;
            };

            var behaviors = serviceProvider.GetServices<IPipelineBehavior<TCommand, TResponse>>().Reverse();
            foreach (var behavior in behaviors)
            {
                var next = handlerDelegate;
                handlerDelegate = (ct) => behavior.HandleAsync(command, next, ct);
            }

            return await handlerDelegate(cancellationToken).ConfigureAwait(false);
        }
    }
}
