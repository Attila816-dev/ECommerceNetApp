using System.Net;
using System.Text.Json;

namespace ECommerceNetApp.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message).ConfigureAwait(false);
            }
            catch (KeyNotFoundException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, ex.Message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred: " + ex.Message).ConfigureAwait(false);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsJsonAsync(message);
        }
    }
}
