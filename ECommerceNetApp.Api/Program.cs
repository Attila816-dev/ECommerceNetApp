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

var builder = WebApplication.CreateBuilder(args);

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

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseErrorHandlingMiddleware();

app.MapControllers();

app.Run();

/// <summary>
/// This is used in integration test.
/// </summary>
[SuppressMessage("Design", "CA1052:Type 'Program' is a static holder type but is neither static nor NotInheritable", Justification = "Required for partial class usage in integration tests.")]
public partial class Program
{
}