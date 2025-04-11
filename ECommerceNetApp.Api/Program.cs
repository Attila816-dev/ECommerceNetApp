using ECommerceNetApp.Persistence;
using ECommerceNetApp.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddECommerceRepositories(builder.Configuration);
builder.Services.AddECommerceServices();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
