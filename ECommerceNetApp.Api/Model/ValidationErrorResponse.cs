using FluentValidation;

namespace ECommerceNetApp.Api.Model
{
    public class ValidationErrorResponse : ErrorResponse
    {
        public ValidationErrorResponse()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            Title = "One or more validation errors occurred.";
            Status = 400;
        }

        public static ValidationErrorResponse CreateValidationErrorResponse(HttpContext context, ValidationException validationException)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(validationException);

            var validationErrorResponse = new ValidationErrorResponse
            {
                TraceId = context.TraceIdentifier,
                Detail = "Please refer to the errors property for additional details.",
            };

            foreach (var error in validationException.Errors)
            {
                if (!validationErrorResponse.Errors!.TryGetValue(error.PropertyName, out string[]? value))
                {
                    validationErrorResponse.Errors[error.PropertyName] = new string[] { error.ErrorMessage };
                }
                else
                {
                    var existingErrors = value.ToList();
                    existingErrors.Add(error.ErrorMessage);
                    validationErrorResponse.Errors[error.PropertyName] = existingErrors.ToArray();
                }
            }

            return validationErrorResponse;
        }
    }
}
