using System.Diagnostics;

namespace ECommerceNetApp.ApiGateway.Middleware
{
    public class RequestLoggingMiddleware
    {
        private static readonly Action<ILogger, string, string, string?, Exception?> LogIncomingRequest =
            LoggerMessage.Define<string, string, string?>(
                LogLevel.Information,
                new EventId(1, nameof(RequestLoggingMiddleware)),
                "[Gateway] Incoming Request: {RequestMethod}, {RequestPath}, TraceId: {TraceId}");

        private static readonly Action<ILogger, int, string?, Exception?> LogOutgoingResponse =
            LoggerMessage.Define<int, string?>(
                LogLevel.Information,
                new EventId(2, nameof(RequestLoggingMiddleware)),
                "[Gateway] Outgoing Response: {StatusCode}, TraceId: {TraceId}");

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
            LogIncomingRequest(_logger, context.Request.Method, context.Request.Path, traceId, null);

            await _next(context).ConfigureAwait(false);

            LogOutgoingResponse(_logger, context.Response.StatusCode, traceId, null);
        }
    }
}
