using EFlow.Common.Messaging.Init;
using EFlow.Notifications.Messaging;
using EFlow.Notifications.Services;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((_, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddNotificationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider
        .GetRequiredService<TopicInitializer>()
        .CreateMissingTopicsAsync();

await app.RunAsync();