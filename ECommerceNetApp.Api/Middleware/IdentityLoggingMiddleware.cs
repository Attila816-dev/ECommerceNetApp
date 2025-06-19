using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ECommerceNetApp.Api.Middleware
{
    public class IdentityLoggingMiddleware
    {
        private const string UnknownValue = "Unknown";

        private static readonly Action<ILogger, string, string, string, string, string, string, Exception?> LogIdentityDetailsMessage =
            LoggerMessage.Define<string, string, string, string, string, string>(
                LogLevel.Information,
                new EventId(1, nameof(IdentityLoggingMiddleware)),
                "Identity Access - User: {UserEmail} ({UserName}), Role: {UserRole}, JTI: {TokenId}, Request: {Method} {Path}");

        private static readonly Action<ILogger, Exception> LogIdentityErrorMessage =
            LoggerMessage.Define(LogLevel.Error, new EventId(1, nameof(ErrorHandlingMiddleware)), "Failed to log identity details");

        private readonly RequestDelegate _next;
        private readonly ILogger<IdentityLoggingMiddleware> _logger;

        public IdentityLoggingMiddleware(RequestDelegate next, ILogger<IdentityLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log identity details if user is authenticated
            ArgumentNullException.ThrowIfNull(context);
            if (context.User.Identity?.IsAuthenticated == true)
            {
                LogIdentityDetails(context);
            }

            await _next(context).ConfigureAwait(false);
        }

        private void LogIdentityDetails(HttpContext context)
        {
            try
            {
                var user = context.User;
                var userEmail = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value ?? UnknownValue;
                var userName = user.FindFirst(ClaimTypes.Name)?.Value ?? UnknownValue;
                var userRole = user.FindFirst(ClaimTypes.Role)?.Value ?? UnknownValue;
                var jti = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? UnknownValue;

                var requestPath = context.Request.Path.Value ?? UnknownValue;
                var requestMethod = context.Request.Method;

                LogIdentityDetailsMessage(_logger, userEmail, userName, userRole, jti, requestMethod, requestPath, null);
            }
            catch (Exception ex)
            {
                LogIdentityErrorMessage(_logger, ex);
            }
        }
    }
}
