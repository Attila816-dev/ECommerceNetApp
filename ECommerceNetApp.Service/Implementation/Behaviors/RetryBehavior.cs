using ECommerceNetApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;

namespace ECommerceNetApp.Service.Implementation.Behaviors
{
    public class RetryBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private static readonly Action<ILogger, int, string, double, string, Exception?> LogNotificationHandlerRegistration =
            LoggerMessage.Define<int, string, double, string>(
            LogLevel.Warning,
            new EventId(1, nameof(RetryBehavior<TRequest, TResponse>)),
            "Retry {RetryCount} for {RequestName} after {Delay}s due to error: {Message}");

        private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;

        public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            var policy = Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        LogNotificationHandlerRegistration.Invoke(_logger, retryCount, typeof(TRequest).Name, timeSpan.TotalSeconds, exception.Message, exception);
                    });

            return await policy.ExecuteAsync(() => next()).ConfigureAwait(false);
        }
    }
}
