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
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <returns>Response object.</returns>
        Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asnchronously sends a command to the appropriate handler.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that represents the send operation.</returns>
        Task SendAsync(ICommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously sends a command to the appropriate handler.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <returns>Response object.</returns>
        Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    }
}
