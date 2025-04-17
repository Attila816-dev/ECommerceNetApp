using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(
                ILogger<LoggingBehavior<TRequest, TResponse>> logger)
                : IPipelineBehavior<TRequest, TResponse>
                where TRequest : notnull
    {
        private static readonly Action<ILogger, string, string, string, Exception?> LogHandlingRequest =
            LoggerMessage.Define<string, string, string>(
                LogLevel.Information,
                new EventId(1, nameof(LoggingBehavior<TRequest, TResponse>)),
                "Handling request {RequestName} [{RequestGuid}] with parameters: {RequestParameters}");

        private static readonly Action<ILogger, string, string, double, Exception?> LogHandledRequest =
            LoggerMessage.Define<string, string, double>(
                LogLevel.Information,
                new EventId(2, nameof(LoggingBehavior<TRequest, TResponse>)),
                "Handled request {RequestName} [{RequestGuid}]; Execution time={ExecutionTime}sec");

        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));
            var requestName = request.GetType().Name;
            var requestGuid = Guid.NewGuid().ToString();
            TResponse response;

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var requestParameters = JsonSerializer.Serialize(request);
                LogHandlingRequest(_logger, requestName, requestGuid, requestParameters, null);

                response = await next(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                stopwatch.Stop();
                LogHandledRequest(_logger, requestName, requestGuid, stopwatch.ElapsedMilliseconds / 1000.0, null);
            }

            return response;
        }
    }
}
