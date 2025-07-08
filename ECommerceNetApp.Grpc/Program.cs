using ECommerceNetApp.Domain.Options;
using ECommerceNetApp.Grpc.Services;
using ECommerceNetApp.Persistence.Extensions;
using ECommerceNetApp.Service.Extensions;
using ECommerceNetApp.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 4 * 1024 * 1024; // 4MB
    options.MaxSendMessageSize = 4 * 1024 * 1024; // 4MB
});

builder.Services.AddGrpcReflection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDispatcher();
builder.Services.AddEventBus(builder.Configuration.GetSection("EventBus").Get<EventBusOptions>()!);
builder.Services.AddCartDb(builder.Configuration);

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();

// Map gRPC services
app.MapGrpcService<CartGrpcService>();

// Enable gRPC reflection for development (helps with BloomRPC)
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
