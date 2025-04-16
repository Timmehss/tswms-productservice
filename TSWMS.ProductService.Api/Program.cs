#region Usings

using RabbitMQ.Client;
using System.Text.Json;
using TSWMS.ProductService.Api.MappingProfiles;
using TSWMS.ProductService.Configurations;
using TSWMS.ProductService.Data;
using TSWMS.ProductService.Shared.Interfaces;

#endregion

var builder = WebApplication.CreateBuilder(args);

// Get Environment
var environment = builder.Environment.EnvironmentName;

// Configure App Configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

// Add CORS Policy
builder.Services.AddCors(o => o.AddPolicy("TSWMSPolicy", builder =>
{
    builder.SetIsOriginAllowed((host) => true)
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
}));

// Configure AutoMapper Profiles
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ProductMappingProfile>();
});

// Configure EntityFramework DbContext
builder.Services.ConfigureUserDbContext(builder.Configuration);

// Configure Managers & Repositories
builder.Services.ConfigureManagers();
builder.Services.ConfigureRepositories();

builder.Services.AddSingleton<IConnectionFactory>(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = "localhost",
        UserName = "guest",
        Password = "guest"
    };
    return factory;
});

// Register RabbitMQ Publisher
builder.Services.AddScoped<IProductPriceListener, ProductPriceListener>();

// Additional service registrations
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize RabbitMQ Listener within async context
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var listener = services.GetRequiredService<IProductPriceListener>();

    // Initialize the listener asynchronously
    await listener.InitializeAsync();
}

app.UseCors("TSWMSPolicy");

// Configure request pipeline
if (app.Environment.IsDevelopment() || environment == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
