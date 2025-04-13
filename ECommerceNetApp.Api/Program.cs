using System.Diagnostics.CodeAnalysis;
using ECommerceNetApp.Api;
using ECommerceNetApp.Api.Extensions;
using ECommerceNetApp.Persistence.Extensions;
using ECommerceNetApp.Persistence.Implementation;
using ECommerceNetApp.Service.Commands;
using ECommerceNetApp.Service.Extensions;
using ECommerceNetApp.Service.Implementation.Behaviors;
using ECommerceNetApp.Service.Implementation.Validators;
using FluentValidation;
using MediatR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container.
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(AddCartItemCommand).Assembly);
});
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

builder.Services.AddECommerceRepositories(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<CartItemValidator>();
builder.Services.AddECommerceServices();

builder.Services.AddHealthChecks()
    .AddCheck<CartDbHealthCheck>("cartdb_health_check");
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ECommerceNetApp API",
        Version = "v1",
        Description = "API for ECommerceNetApp",
    });
});

var app = builder.Build();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<CartDbInitializer>();
    initializer.InitializeDatabaseAsync().Wait();

    var seeder = scope.ServiceProvider.GetRequiredService<CartDbSampleDataSeeder>();
    seeder.SeedSampleDataAsync().Wait();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
    });
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
    };

    // Customize the message template
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseErrorHandlingMiddleware();

app.MapControllers();

#pragma warning disable CA1031 // Do not catch general exception types
try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
#pragma warning restore CA1031 // Do not catch general exception types

/// <summary>
/// This is used in integration test.
/// </summary>
[SuppressMessage("Design", "CA1052:Type 'Program' is a static holder type but is neither static nor NotInheritable", Justification = "Required for partial class usage in integration tests.")]
public partial class Program
{
}