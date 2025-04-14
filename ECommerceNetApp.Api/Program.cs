using System.Diagnostics.CodeAnalysis;
using ECommerceNetApp.Api;
using ECommerceNetApp.Api.Extensions;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Persistence.Extensions;
using ECommerceNetApp.Persistence.Implementation;
using ECommerceNetApp.Service.Commands;
using ECommerceNetApp.Service.Extensions;
using ECommerceNetApp.Service.Implementation.Behaviors;
using ECommerceNetApp.Service.Implementation.Validators;
using FluentValidation;
using MediatR;
using Serilog;

Action<Microsoft.Extensions.Logging.ILogger, Exception> logSeedDatabaseErrorAction =
    LoggerMessage.Define(
        LogLevel.Error,
        new EventId(0, nameof(InitializeAndSeedDatabasesAsync)),
        "An error occurred while migrating or seeding the database.");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Bind CartDbOptions
builder.Services.Configure<CartDbOptions>(
    builder.Configuration.GetSection(nameof(CartDbOptions)));

// Bind ProductCatalogDbOptions
builder.Services.Configure<ProductCatalogDbOptions>(
    builder.Configuration.GetSection(nameof(ProductCatalogDbOptions)));

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

await InitializeAndSeedDatabasesAsync(app).ConfigureAwait(false);

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
    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
#pragma warning restore CA1031 // Do not catch general exception types

async Task InitializeAndSeedDatabasesAsync(WebApplication app, CancellationToken cancellationToken = default)
{
    using (var scope = app.Services.CreateScope())
    {
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            var initializer = scope.ServiceProvider.GetRequiredService<CartDbInitializer>();
            await initializer.InitializeDatabaseAsync(cancellationToken).ConfigureAwait(false);

            var cartDbSeeder = scope.ServiceProvider.GetRequiredService<CartDbSampleDataSeeder>();
            await cartDbSeeder.SeedSampleDataAsync(cancellationToken).ConfigureAwait(false);

            var productCatalogDbSeeder = scope.ServiceProvider.GetRequiredService<ProductCatalogDbSampleDataSeeder>();
            await productCatalogDbSeeder.SeedSampleDataAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logSeedDatabaseErrorAction.Invoke(logger, ex);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}

/// <summary>
/// This is used in integration test.
/// </summary>
[SuppressMessage("Design", "CA1052:Type 'Program' is a static holder type but is neither static nor NotInheritable", Justification = "Required for partial class usage in integration tests.")]
public partial class Program
{
}