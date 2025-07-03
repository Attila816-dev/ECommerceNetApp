using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ECommerceNetApp.Api.Extensions
{
    public static class TelemetryConfiguration
    {
        public static readonly ActivitySource ActivitySource = new("ECommerceNetApp.Api");

        public static void ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "ECommerceNetApp.Api";
            var serviceVersion = configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";
            var connectionString = configuration["ApplicationInsights:ConnectionString"];

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                        ["service.instance.id"] = Environment.MachineName,
                        ["service.namespace"] = "ECommerceNetApp.Api",
                        ["service.name"] = serviceName,
                        ["service.version"] = serviceVersion,
                    }))
                .WithTracing(builder =>
                {
                    builder
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.Filter = httpContext =>
                            {
                                // Skip health check endpoints
                                return !httpContext.Request.Path.Value?.Contains("/health", StringComparison.InvariantCultureIgnoreCase) ?? true;
                            };
                            options.EnrichWithHttpRequest = (activity, httpRequest) =>
                            {
                                activity.SetTag("http.request.body.size", httpRequest.ContentLength);
                                activity.SetTag("http.request.header.user-agent", httpRequest.Headers.UserAgent.ToString());
                            };
                            options.EnrichWithHttpResponse = (activity, httpResponse) =>
                            {
                                activity.SetTag("http.response.body.size", httpResponse.ContentLength);
                            };
                        })
                        .AddEntityFrameworkCoreInstrumentation(options =>
                        {
                            options.SetDbStatementForText = true;
                        })
                        .AddSource(ActivitySource.Name)
                        .SetSampler(new TraceIdRatioBasedSampler(1.0)) // 100% sampling for development
                        .AddConsoleExporter(); // For development debugging

                    // Add Azure Monitor if connection string is provided
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        builder.AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString = connectionString;
                        });
                    }
                })
                .WithMetrics(builder =>
                {
                    builder
                        .AddAspNetCoreInstrumentation()
                        .AddMeter("ECommerceNetApp.Api") // Custom metrics
                        .AddConsoleExporter();

                    // Add Azure Monitor if connection string is provided
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        builder.AddAzureMonitorMetricExporter(options =>
                        {
                            options.ConnectionString = connectionString;
                        });
                    }
                });
        }
    }
}
