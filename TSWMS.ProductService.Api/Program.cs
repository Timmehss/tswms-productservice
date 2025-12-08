#region Usings

using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TSWMS.ProductService.Api.MappingProfiles;
using TSWMS.ProductService.Api.Middlewares;
using TSWMS.ProductService.Configurations;
using TSWMS.ProductService.Data;
using TSWMS.ProductService.Data.Publishers;
using TSWMS.ProductService.Shared.Interfaces;

#endregion

var builder = WebApplication.CreateBuilder(args);

// Get Environment
var environment = builder.Environment.EnvironmentName;

Console.WriteLine($"Initial environment: {environment}");

// Configure App Configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

// Configure Dapr Services & Endpoints
builder.Configuration.AddJsonFile("dapr.config.json", optional: false, reloadOnChange: true);

// Add CORS Policy
builder.Services.AddCors(o => o.AddPolicy("TSWMSPolicy", builder =>
{
    builder.SetIsOriginAllowed((host) => true)
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
}));

// Add Dapr
builder.Services.AddDaprClient();

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

builder.Services.AddScoped<IEventPublisher, DaprEventPublisher>();

// Additional service registrations
builder.Services.AddControllers()
    .AddDapr()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//string? secretKey;

//if (environment == "Test" || environment == "Docker")
//{
//    // Set the key only if it's not already set
//    secretKey = Environment.GetEnvironmentVariable("HMAC_SECRET_KEY");

//    if (string.IsNullOrEmpty(secretKey))
//    {
//        secretKey = "qWX4IlPFoIKLeSoiiT1JBAl7KvzIRwVm";
//        Environment.SetEnvironmentVariable("HMAC_SECRET_KEY", secretKey);
//    }
//}
//else
//{
//    // Sign RabbitMQ messages with HMAC.
//    secretKey = Environment.GetEnvironmentVariable("HMAC_SECRET_KEY");
//}

//if (string.IsNullOrEmpty(secretKey))
//{
//    throw new InvalidOperationException("HMAC secret key is missing!");
//}

//builder.Services.Configure<HmacOptions>(options =>
//{
//    options.SecretKey = secretKey!;
//});

var app = builder.Build();

// Apply Database Migrations if it's not in "Test" environment
if (environment != "Test" || environment == "Docker" || environment == "Kubernetes")
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ProductsDbContext>();

        // Apply pending migrations or create the database if it doesn't exist
        dbContext.Database.Migrate();
    }
}

app.UseCors("TSWMSPolicy");

// Exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure request pipeline
if (app.Environment.IsDevelopment() || environment == "Docker" || environment == "Kubernetes")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseCloudEvents();

app.MapControllers();

// Add Dapr subscribe handler
app.MapSubscribeHandler();

app.Run();
