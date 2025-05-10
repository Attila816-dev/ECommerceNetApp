namespace ECommerceNetApp.Domain.Interfaces
{
    /// <summary>
    /// Represents an async continuation for the next task to execute in the pipeline.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <returns>Awaitable task returning a <typeparamref name="TResponse"/>response.</returns>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken = default);
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix

    public interface IPipelineBehavior<in TRequest, TResponse>
        where TRequest : notnull
    {
#pragma warning disable CA1716 // Identifiers should not match keywords
        /// <summary>
        /// Pipeline handler. Perform any additional behavior and await the <paramref name="next"/> delegate as necessary.
        /// </summary>
        /// <param name="request">Incoming request.</param>
        /// <param name="next">Awaitable delegate for the next action in the pipeline. Eventually this delegate represents the handler.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Awaitable task returning the <typeparamref name="TResponse"/>Task.</returns>
        Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default);
#pragma warning restore CA1716 // Identifiers should not match keywords
    }
}
