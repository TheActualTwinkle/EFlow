using EFlow.Common.Extensions;
using EFlow.DataImport.Application;
using EFlow.DataImport.Messaging;
using Scalar.AspNetCore;
using Serilog;
using ISystemClock = EFlow.Common.Infrastructure.ISystemClock;
using SystemClock = EFlow.Common.Infrastructure.SystemClock;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

if (builder.Environment.IsOpenApiGenerator())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();

    builder.Services.AddControllers();

    var openApiApp = builder.Build();

    openApiApp.MapOpenApi();
    openApiApp.MapControllers();

    await openApiApp.RunAsync();

    return;
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructureClients(builder.Configuration);
builder.Services.AddSingleton<ISystemClock, SystemClock>();

builder.Host.UseSerilog((_, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration));

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
                             .GetRequiredSection("Cors:AllowedOrigins")
                             .Get<string[]>()
                         ?? throw new InvalidOperationException("Allowed origins not configured");
    
    options.AddPolicy(
        "AllowFrontend",
        policyBuilder => policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.MapOpenApi();

if (app.Environment.IsDevelopment())
    app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();
