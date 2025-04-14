using System.Diagnostics.CodeAnalysis;
using ECommerceNetApp.Api.Extensions;
using ECommerceNetApp.Domain;
using ECommerceNetApp.Persistence;
using ECommerceNetApp.Service;

Action<ILogger, Exception> logSeedDatabaseErrorAction =
    LoggerMessage.Define(
        LogLevel.Error,
        new EventId(0, nameof(InitializeAndSeedDatabasesAsync)),
        "An error occurred while migrating or seeding the database.");

var builder = WebApplication.CreateBuilder(args);

// Bind CartDbOptions
builder.Services.Configure<CartDbOptions>(
    builder.Configuration.GetSection(nameof(CartDbOptions)));

// Bind ProductCatalogDbOptions
builder.Services.Configure<ProductCatalogDbOptions>(
    builder.Configuration.GetSection(nameof(ProductCatalogDbOptions)));

// Add services to the container.
builder.Services.AddECommerceRepositories(builder.Configuration);
builder.Services.AddECommerceServices();

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

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseErrorHandlingMiddleware();

app.MapControllers();

await app.RunAsync().ConfigureAwait(false);

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