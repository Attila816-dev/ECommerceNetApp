using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using ECommerceNetApp.Api.Authorization;
using ECommerceNetApp.Api.Extensions;
using ECommerceNetApp.Api.HealthCheck;
using ECommerceNetApp.Api.Services;
using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Persistence.Extensions;
using ECommerceNetApp.Service.Extensions;
using ECommerceNetApp.Service.Implementation.Behaviors;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Validators.Cart;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace ECommerceNetApp.Api
{
    /// <summary>
    /// This is used in integration test.
    /// </summary>
    [SuppressMessage("Design", "CA1052:Type 'Program' is a static holder type but is neither static nor NotInheritable", Justification = "Required for partial class usage in integration tests.")]
    public partial class Program
    {
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

            app.UseErrorHandlingMiddleware();
            app.UseIdentityLoggingMiddleware();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Run the application
            await RunApplicationAsync(app).ConfigureAwait(false);
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddProblemDetails(options =>
            {
                // Customize problem details if needed
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = context.HttpContext.Request.Path;
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                };
            });

            builder.Services.Configure<CartDbOptions>(builder.Configuration.GetSection(nameof(CartDbOptions)));
            builder.Services.Configure<ProductCatalogDbOptions>(builder.Configuration.GetSection(nameof(ProductCatalogDbOptions)));
            builder.Services.Configure<EventBusOptions>(builder.Configuration.GetSection(EventBusOptions.SectionName));
            builder.Services.AddOptions<JwtOptions>().Bind(builder.Configuration.GetSection("Jwt")).ValidateDataAnnotations().ValidateOnStart();
            builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

            var eventBusOptions = builder.Configuration.GetSection(EventBusOptions.SectionName).Get<EventBusOptions>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDispatcher();
            builder.Services.AddAuthenticationServices();
            builder.Services.AddEventBus(eventBusOptions!);
            builder.Services.AddHostedService<EventBusBackgroundService>();

            builder.Services.AddScoped<IHateoasLinkService, HateoasLinkService>();
            builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
            builder.Services.AddCartDb(builder.Configuration);
            builder.Services.AddProductCatalogDb(builder.Configuration);
            builder.Services.AddValidatorsFromAssemblyContaining<AddCartItemCommandValidator>();
            builder.Services.AddHostedService<DatabaseInitializer>();
            ConfigureHealthCheck(builder);
            ConfigureAuthentication(builder);
        }

        private static void ConfigureAuthentication(WebApplicationBuilder builder)
        {
            // Configure JWT authentication
            var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
            ArgumentNullException.ThrowIfNull(jwtOptions);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtOptions.GetSecretKeyBytes()),
                    ClockSkew = TimeSpan.Zero,
                };

                // Add event to enrich JWT claims with permissions
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var rolePermissionService = context.HttpContext.RequestServices.GetRequiredService<IRolePermissionService>();
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

                        if (claimsIdentity != null)
                        {
                            // Get the user's role from the JWT token
                            var roleClaim = claimsIdentity.FindFirst(ClaimTypes.Role);
                            if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out var userRole))
                            {
                                // Get permissions for the user's role
                                var permissions = rolePermissionService.GetPermissionsForRole(userRole);

                                // Add permission claims
                                foreach (var permission in permissions)
                                {
                                    claimsIdentity.AddClaim(new Claim("permission", $"{permission.Action}:{permission.Resource}"));
                                }
                            }
                        }

                        return Task.CompletedTask;
                    },
                };
            });

            // Register permission-based authorization services
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // Configure authorization without role-based policies
            builder.Services.AddAuthorization();
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
            builder.Services
                .AddApiVersioning(options =>
                {
                    options.ApiVersionReader = ApiVersionReader.Combine(
                        new UrlSegmentApiVersionReader(),
                        new HeaderApiVersionReader("X-API-Version"),
                        new MediaTypeApiVersionReader("v"));

                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Get all API version descriptors
                var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                // Add a swagger document for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(
                        description.GroupName,
                        new OpenApiInfo
                        {
                            Title = $"E-commerce API {description.ApiVersion}",
                            Version = description.ApiVersion.ToString(),
                            Description = description.IsDeprecated
                                ? "This API version has been deprecated."
                                : "E-commerce API",
                        });
                }

                // Include XML comments if available
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Add JWT Authentication to Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
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

                var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                app.UseSwaggerUI(options =>
                {
                    // Build a swagger endpoint for each discovered API version in reverse order
                    // This ensures newest versions appear first in the dropdown
                    foreach (var groupName in apiVersionDescriptionProvider.ApiVersionDescriptions.Select(d => d.GroupName))
                    {
                        options.SwaggerEndpoint(
                            $"/swagger/{groupName}/swagger.json",
                            $"E-commerce API {groupName.ToUpperInvariant()}");
                    }

                    // Set document expansion to list to show all operations by default
                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);

                    options.DisplayRequestDuration();
                    options.ConfigObject.DefaultModelsExpandDepth = -1; // Hide schemas section by default
                });
            }
            else
            {
                app.UseExceptionHandler("/error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios.
                app.UseHsts();
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