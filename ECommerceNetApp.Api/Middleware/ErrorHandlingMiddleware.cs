using System.Net;
using ECommerceNetApp.Domain.Exceptions;
using ECommerceNetApp.Domain.Exceptions.Cart;
using FluentValidation;

namespace ECommerceNetApp.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private static readonly Action<ILogger, HttpStatusCode, string, Exception> LogErrorMessage =
            LoggerMessage.Define<HttpStatusCode, string>(
                LogLevel.Error,
                new EventId(1, nameof(ErrorHandlingMiddleware)),
                "An error occurred while processing the request. StatusCode: {StatusCode}, TraceId: {TraceId}");

        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (CartItemNotFoundException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is ArgumentException or DomainException or ValidationException)
            {
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, ex).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, ex).ConfigureAwait(false);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, Exception exception)
        {
            // Log the exception using LoggerMessage delegate
            LogErrorMessage(_logger, statusCode, context.TraceIdentifier, exception);

            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsJsonAsync(new
            {
                error = statusCode == HttpStatusCode.InternalServerError ? "An unexpected error occurred." : exception.Message,
                traceId = context.TraceIdentifier,
            });
        }
    }
}
