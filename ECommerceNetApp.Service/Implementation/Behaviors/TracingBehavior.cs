using System.Diagnostics;
using System.Text.Json;
using ECommerceNetApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceNetApp.Service.Implementation.Behaviors
{
    public class TracingBehavior<TRequest, TResponse>(
        ILogger<TracingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private static readonly Action<ILogger, string, string, string?, Exception?> LogStartOperation =
            LoggerMessage.Define<string, string, string?>(
                LogLevel.Information,
                new EventId(1, nameof(TracingBehavior<TRequest, TResponse>)),
                "Starting {OperationType} {RequestName} with CorrelationId {CorrelationId}");

        private static readonly Action<ILogger, string, string, long, string?, Exception?> LogSuccessfulOperation =
            LoggerMessage.Define<string, string, long, string?>(
                LogLevel.Information,
                new EventId(2, nameof(TracingBehavior<TRequest, TResponse>)),
                "Completed {OperationType} {RequestName} in {ElapsedMilliseconds}ms with CorrelationId {CorrelationId}");

        private static readonly Action<ILogger, string, string, long, string?, Exception?> LogFailedOperation =
            LoggerMessage.Define<string, string, long, string?>(
            LogLevel.Error,
            new EventId(3, nameof(TracingBehavior<TRequest, TResponse>)),
            "Error in {OperationType} {RequestName} after {ElapsedMilliseconds}ms with CorrelationId {CorrelationId}");

        private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        // List of patterns that indicate sensitive data
        private static readonly string[] SensitivePatterns =
        [
            "password", "token", "secret", "key", "auth", "login", "credential",
            "payment", "card", "credit", "debit", "account", "bank", "ssn", "sin",
        ];

        private readonly ILogger<TracingBehavior<TRequest, TResponse>> _logger = logger;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(next);

            var requestName = typeof(TRequest).Name;
            var responseName = typeof(TResponse).Name;
            var isCommand = requestName.EndsWith("Command", StringComparison.InvariantCultureIgnoreCase);
            var isQuery = requestName.EndsWith("Query", StringComparison.InvariantCultureIgnoreCase);

            var operationType = isCommand ? "Command" : (isQuery ? "Query" : "Operation");
            var activityName = $"{operationType}.{requestName}";

            using var activity = ServiceTelemetry.ActivitySource.StartActivity(activityName);
            SetActivityTags(request, requestName, operationType, activity);
            var correlationId = activity?.GetTagItem("correlation.id")?.ToString();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                LogStartOperation(_logger, operationType, requestName, correlationId, null);
                var response = await next(cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();
                SetSuccessfulResponseActivityTags(responseName, activity, stopwatch, response);
                LogSuccessfulOperation(_logger, operationType, requestName, stopwatch.ElapsedMilliseconds, correlationId, null);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                SetActvityErrorTags(activity, stopwatch, ex);
                LogFailedOperation(_logger, operationType, requestName, stopwatch.ElapsedMilliseconds, correlationId, ex);
                throw;
            }
        }

        private static void SetActivityTags(TRequest request, string requestName, string operationType, Activity? activity)
        {
            if (activity != null)
            {
                // Set standard tags
                activity.SetTag("operation.type", operationType);
                activity.SetTag("operation.name", requestName);
                activity.SetTag("operation.category", "MediatR");
                activity.SetTag("request.type", typeof(TRequest).FullName);
                activity.SetTag("response.type", typeof(TResponse).FullName);

                // Add correlation context from current activity
                var correlationId = Activity.Current?.GetBaggageItem("correlation.id") ?? Guid.NewGuid().ToString();
                activity.SetTag("correlation.id", correlationId);

                // Add user context if available
                var userId = Activity.Current?.GetBaggageItem("user.id") ?? "anonymous";
                activity.SetTag("user.id", userId);

                // Add request details (be careful with sensitive data)
                try
                {
                    var requestJson = JsonSerializer.Serialize(request, CachedJsonSerializerOptions);

                    // Only add request body for non-sensitive operations
                    if (!ContainsSensitiveData(requestName))
                    {
                        activity.SetTag("request.body", requestJson);
                    }

                    activity.SetTag("request.size", requestJson.Length);
                }
                catch (Exception ex)
                {
                    activity.SetTag("request.body", "Failed to serialize");
                    activity.SetTag("request.serialization.error", ex.Message);
                }
            }
        }

        private static void SetSuccessfulResponseActivityTags(string responseName, Activity? activity, Stopwatch stopwatch, TResponse? response)
        {
            if (activity != null)
            {
                activity.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);
                activity.SetTag("success", true);
                activity.SetStatus(ActivityStatusCode.Ok);

                // Add response details (be careful with sensitive data)
                try
                {
                    if (response != null && !ContainsSensitiveData(responseName))
                    {
                        var responseJson = JsonSerializer.Serialize(response, CachedJsonSerializerOptions);
                        activity.SetTag("response.size", responseJson.Length);
                    }
                }
                catch (Exception ex)
                {
                    activity.SetTag("response.serialization.error", ex.Message);
                }
            }
        }

        private static void SetActvityErrorTags(Activity? activity, Stopwatch stopwatch, Exception ex)
        {
            if (activity != null)
            {
                activity.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);
                activity.SetTag("success", false);
                activity.SetTag("error.type", ex.GetType().Name);
                activity.SetTag("error.message", ex.Message);
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.AddException(ex);
            }
        }

        private static bool ContainsSensitiveData(string typeName)
        {
            return SensitivePatterns.Any(pattern => typeName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }
    }
}
