using System.Diagnostics.CodeAnalysis;
using ECommerceNetApp.Api.Extensions;
using ECommerceNetApp.Api.HealthCheck;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Persistence.Extensions;
using ECommerceNetApp.Persistence.Implementation.Cart;
using ECommerceNetApp.Persistence.Implementation.ProductCatalog;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.Extensions;
using ECommerceNetApp.Service.Implementation.Behaviors;
using ECommerceNetApp.Service.Implementation.Validators.Cart;
using FluentValidation;
using MediatR;
using Serilog;

namespace ECommerceNetApp.Api
{
    /// <summary>
    /// This is used in integration test.
    /// </summary>
    [SuppressMessage("Design", "CA1052:Type 'Program' is a static holder type but is neither static nor NotInheritable", Justification = "Required for partial class usage in integration tests.")]
    public partial class Program
    {
        private static Action<Microsoft.Extensions.Logging.ILogger, Exception> _logSeedDatabaseErrorAction =
            LoggerMessage.Define(
                LogLevel.Error,
                new EventId(0, nameof(InitializeAndSeedDatabasesAsync)),
                "An error occurred while migrating or seeding the database.");

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure services
            ConfigureLogging(builder);
            ConfigureServices(builder);
            builder.Services.AddControllers();
            ConfigureSwagger(builder);

            var app = builder.Build();

            // Configure middleware and request pipeline
            app.MapHealthChecks("/health");
            UseSwagger(app);

            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
                };

                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            });

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseErrorHandlingMiddleware();
            app.MapControllers();

            // Initialize and seed databases
            await InitializeAndSeedDatabasesAsync(app).ConfigureAwait(false);

            // Run the application
            await RunApplicationAsync(app).ConfigureAwait(false);
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.Configure<CartDbOptions>(builder.Configuration.GetSection(nameof(CartDbOptions)));
            builder.Services.Configure<ProductCatalogDbOptions>(builder.Configuration.GetSection(nameof(ProductCatalogDbOptions)));
            builder.Services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(AddCartItemCommand).Assembly);
            });

            builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            builder.Services.AddECommerceRepositories(builder.Configuration);
            builder.Services.AddValidatorsFromAssemblyContaining<CartItemValidator>();
            builder.Services.AddECommerceServices();
            ConfigureHealthCheck(builder);
        }

        private static void ConfigureHealthCheck(WebApplicationBuilder builder)
        {
            builder.Services.AddHealthChecks()
                .AddCheck<CartDbHealthCheck>("CartDb_health_check")
                .AddCheck<ProductCatalogDbHealthCheck>("ProductCatalogDb_health_check");
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());
        }

        private static void ConfigureSwagger(WebApplicationBuilder builder)
        {
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
        }

        private static void UseSwagger(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(c =>
                {
                    c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
                });
                app.UseSwaggerUI();
            }
        }

        private static async Task InitializeAndSeedDatabasesAsync(WebApplication app, CancellationToken cancellationToken = default)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var initializer = scope.ServiceProvider.GetRequiredService<CartDbInitializer>();
                await initializer.InitializeDatabaseAsync(cancellationToken).ConfigureAwait(false);

                var cartDbSeeder = scope.ServiceProvider.GetRequiredService<CartDbSampleDataSeeder>();
                await cartDbSeeder.SeedSampleDataAsync(cancellationToken).ConfigureAwait(false);

                var productCatalogDbSeeder = scope.ServiceProvider.GetRequiredService<ProductCatalogDbSampleDataSeeder>();
                await productCatalogDbSeeder.SeedSampleDataAsync(cancellationToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logSeedDatabaseErrorAction.Invoke(logger, ex);
            }
        }

        private static async Task RunApplicationAsync(WebApplication app)
        {
            try
            {
                Log.Information("Starting web host");
                await app.RunAsync().ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                await Log.CloseAndFlushAsync().ConfigureAwait(false);
            }
        }
    }
}