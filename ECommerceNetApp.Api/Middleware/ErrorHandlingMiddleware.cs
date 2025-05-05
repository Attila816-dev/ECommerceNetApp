using System.Net;
using System.Text.Json;
using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Domain.Exceptions;
using FluentValidation;

namespace ECommerceNetApp.Api.Middleware
{
    public class ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        private static readonly Action<ILogger, HttpStatusCode, string, Exception> LogErrorMessage =
            LoggerMessage.Define<HttpStatusCode, string>(
                LogLevel.Error,
                new EventId(1, nameof(ErrorHandlingMiddleware)),
                "An error occurred while processing the request. StatusCode: {StatusCode}, TraceId: {TraceId}");

        private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly RequestDelegate _next = next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;
        private readonly IHostEnvironment _environment = environment;

        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            ErrorResponse errorResponse;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = ValidationErrorResponse.CreateValidationErrorResponse(context, validationException);
                    break;

                case DomainException domainException:
                    errorResponse = ErrorResponse.CreateErrorResponse(context, domainException);
                    statusCode = (HttpStatusCode)errorResponse.Status;
                    break;

                case ArgumentException argumentException:
                    errorResponse = ErrorResponse.CreateErrorResponse(context, argumentException);
                    statusCode = (HttpStatusCode)errorResponse.Status;
                    break;

                case InvalidOperationException invalidOperationException:
                    errorResponse = ErrorResponse.CreateErrorResponse(context, invalidOperationException);
                    statusCode = (HttpStatusCode)errorResponse.Status;
                    break;

                default:
                    errorResponse = ErrorResponse.CreateErrorResponse(context, exception, _environment.IsDevelopment());
                    statusCode = (HttpStatusCode)errorResponse.Status;
                    break;
            }

            LogErrorMessage(_logger, statusCode, context.TraceIdentifier, exception);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var responseJson = JsonSerializer.Serialize(errorResponse, CachedJsonSerializerOptions);
            return context.Response.WriteAsync(responseJson);
        }
    }
}
