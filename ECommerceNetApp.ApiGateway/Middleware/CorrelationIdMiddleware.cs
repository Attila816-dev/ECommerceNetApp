using Microsoft.Extensions.Primitives;

namespace ECommerceNetApp.ApiGateway.Middleware
{
    public class CorrelationIdMiddleware
    {
        private static readonly Action<ILogger, StringValues, Exception?> LogCorrelationId =
            LoggerMessage.Define<StringValues>(
                LogLevel.Information,
                new EventId(0, nameof(CorrelationIdMiddleware)),
                "[Gateway] Correlation ID: {CorrelationId}");

        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            // Check for incoming correlation ID
            ArgumentNullException.ThrowIfNull(context);
            const string CorrelationIdHeader = "X-Correlation-ID";
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers[CorrelationIdHeader] = correlationId;
            }

            // Log the correlation ID and process the request
            LogCorrelationId(_logger, correlationId, null);
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            await _next(context).ConfigureAwait(false);
        }
    }
}
