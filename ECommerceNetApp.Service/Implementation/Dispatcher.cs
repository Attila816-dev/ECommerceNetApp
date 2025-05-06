using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service.Implementation
{
    public class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public async Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query, nameof(query));
            var handler = _serviceProvider.GetService<IQueryHandler<IQuery<TResponse>, TResponse>>()
                ?? throw new InvalidOperationException($"Handler for query {query.GetType().Name} not found.");

            var behaviors = serviceProvider.GetServices<IPipelineBehavior<IQuery<TResponse>, TResponse>>().Reverse();

            RequestHandlerDelegate<TResponse> handlerDelegate = (ct) => handler.HandleAsync(query, ct);
            foreach (var behavior in behaviors)
            {
                var next = handlerDelegate;
                handlerDelegate = (ct) => behavior.HandleAsync(query, next, ct);
            }

            return await handlerDelegate(cancellationToken).ConfigureAwait(false);
        }

        public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            var handler = _serviceProvider.GetService<ICommandHandler<ICommand>>()
                ?? throw new InvalidOperationException($"Handler for command {command.GetType().Name} not found.");

            var behaviors = serviceProvider.GetServices<IPipelineBehavior<ICommand, bool>>().Reverse();

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

        public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            var handler = _serviceProvider.GetService<ICommandHandler<ICommand<TResponse>, TResponse>>()
                ?? throw new InvalidOperationException($"Handler for command {command.GetType().Name} not found.");

            var behaviors = serviceProvider.GetServices<IPipelineBehavior<ICommand<TResponse>, TResponse>>().Reverse();

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
