using System.Diagnostics.CodeAnalysis;
using ECommerceNetApp.Api.Extensions;
using ECommerceNetApp.Persistence.Extensions;
using ECommerceNetApp.Persistence.Implementation;
using ECommerceNetApp.Service;

var builder = WebApplication.CreateBuilder(args);

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