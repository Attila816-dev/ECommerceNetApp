using System.Net;
using ECommerceNetApp.Domain.Exceptions;

namespace ECommerceNetApp.Api.Model
{
    public class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public int Status { get; set; }

        public string Detail { get; set; } = string.Empty;

        public string TraceId { get; set; } = string.Empty;

        public Dictionary<string, string[]>? Errors { get; } = new();

        public static ErrorResponse CreateErrorResponse(HttpContext context, DomainException domainException)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(domainException);

            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            if (domainException.StatusCode.HasValue)
            {
                statusCode = domainException.StatusCode.Value;
            }

            string title = "Domain Rule Violation";
            string type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

            if (statusCode == HttpStatusCode.NotFound)
            {
                title = "Resource Not Found";
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            }

            return new ErrorResponse
            {
                Type = type,
                Title = title,
                Status = (int)statusCode,
                Detail = domainException.Message,
                TraceId = context.TraceIdentifier,
            };
        }

        public static ErrorResponse CreateErrorResponse(HttpContext context, ArgumentException argumentException)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(argumentException);
            return new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = argumentException.Message,
                TraceId = context.TraceIdentifier,
            };
        }

        public static ErrorResponse CreateErrorResponse(HttpContext context, InvalidOperationException invalidOperationException)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(invalidOperationException);
            return new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                Title = "Resource Conflict",
                Status = (int)HttpStatusCode.Conflict,
                Detail = invalidOperationException.Message,
                TraceId = context.TraceIdentifier,
            };
        }

        public static ErrorResponse CreateErrorResponse(HttpContext context, Exception exception, bool isDevelopment)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(exception);
            return new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = isDevelopment ? exception.Message : "An unexpected error occurred.",
                TraceId = context.TraceIdentifier,
            };
        }
    }
}
