using System.Reflection;
using ECommerceNetApp.ApiGateway.Middleware;
using Microsoft.Extensions.Options;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService("ECommerceNetApp.ApiGateway", serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0", serviceInstanceId: Environment.MachineName);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, request) =>
                {
                    activity.SetTag("http.request.host", request.Host.Value);
                    activity.SetTag("http.request.user_agent", request.Headers["User-Agent"].ToString());
                    activity.SetTag("http.request.method", request.Method);
                    activity.SetTag("http.request.scheme", request.Scheme);
                    activity.SetTag("http.request.path", request.Path.Value);
                    activity.SetTag("http.request.query_string", request.QueryString.Value);
                    activity.SetTag("http.request.body.size", request.ContentLength ?? 0);
                };

                options.EnrichWithHttpResponse = (activity, response) =>
                {
                    activity.SetTag("http.response.status_code", response.StatusCode);
                    activity.SetTag("http.response.body.size", response.ContentLength ?? 0);
                };
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true; // Record exceptions during HTTP calls
                options.FilterHttpRequestMessage = request =>
                {
                    // Skip health check requests
                    return !request.RequestUri?.AbsolutePath?.Contains("/health", StringComparison.OrdinalIgnoreCase) ?? true;
                };
                options.EnrichWithHttpRequestMessage = (activity, httpRequest) =>
                {
                    activity.SetTag("http.client.request.method", httpRequest.Method.Method);
                    activity.SetTag("http.client.request.url", httpRequest.RequestUri?.ToString());
                    activity.SetTag("http.client.request.body.size", httpRequest.Content?.Headers?.ContentLength);
                };
                options.EnrichWithHttpResponseMessage = (activity, httpResponse) =>
                {
                    activity.SetTag("http.client.response.body.size", httpResponse.Content?.Headers?.ContentLength);
                };
            })
            .AddSource("ECommerceNetApp.Api", "ECommerceNetApp.Service")
            .AddConsoleExporter(); // For development - you'll replace this with Application Insights later
    })
    .WithMetrics(metricsBuilder =>
    {
        metricsBuilder
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter();
    });

// Add OpenTelemetry Logging
builder.Logging.AddOpenTelemetry(loggingBuilder =>
{
    loggingBuilder
        .SetResourceBuilder(resourceBuilder)
        .AddConsoleExporter();
});

// Add Ocelot configuration
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ocelot.SwaggerEndPoints.json", optional: true, reloadOnChange: true);

// Add Ocelot and SwaggerForOcelot services
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// Enable the Swagger UI for Ocelot
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs"; // Swagger aggregation endpoint
});
await app.UseOcelot().ConfigureAwait(false);
await app.RunAsync().ConfigureAwait(false);
