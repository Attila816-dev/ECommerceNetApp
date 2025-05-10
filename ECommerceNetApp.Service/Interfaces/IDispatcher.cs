using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Interfaces
{
    public interface IDispatcher
    {
        /// <summary>
        /// Asynchronously sends a query to the appropriate handler.
        /// </summary>
        /// <param name="query">Query object.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <returns>Response object.</returns>
        Task<TResponse> SendQueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TResponse>;

        /// <summary>
        /// Asnchronously sends a command to the appropriate handler.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <typeparam name="TCommand">Command type.</typeparam>
        /// <returns>A task that represents the send operation.</returns>
        Task SendCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand;

        /// <summary>
        /// Asynchronously sends a command to the appropriate handler.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <typeparam name="TCommand">Command type.</typeparam>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <returns>Response object.</returns>
        Task<TResponse> SendCommandAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : ICommand<TResponse>;
    }
}
