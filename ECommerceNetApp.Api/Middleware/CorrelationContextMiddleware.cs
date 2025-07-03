using System.Diagnostics;

namespace ECommerceNetApp.Api.Middleware
{
    public class CorrelationContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationContextMiddleware> _logger;

        public CorrelationContextMiddleware(RequestDelegate next, ILogger<CorrelationContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
            var spanId = Activity.Current?.SpanId.ToString() ?? Guid.NewGuid().ToString("N")[..16];

            // Get or create correlation ID
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                             ?? context.Request.Headers["X-Request-ID"].FirstOrDefault()
                             ?? Guid.NewGuid().ToString();

            // Set correlation context for the request
            context.Items["CorrelationId"] = correlationId;
            context.Items["TraceId"] = traceId;
            context.Items["SpanId"] = spanId;

            // Add to response headers for client tracking
            context.Response.Headers.Append("X-Correlation-ID", correlationId);
            context.Response.Headers.Append("X-Trace-ID", traceId);

            // Set activity tags
            if (Activity.Current != null)
            {
                Activity.Current.SetTag("correlation.id", correlationId);
                Activity.Current.SetTag("user.id", context.User?.Identity?.Name ?? "anonymous");
                Activity.Current.SetTag("request.id", context.TraceIdentifier);
            }

            // Add to log context
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["TraceId"] = traceId,
                ["SpanId"] = spanId,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method,
            });

            await _next(context).ConfigureAwait(false);
        }
    }
}
