using EFlow.Common.Messaging.Init;
using EFlow.Notifications.Application;
using EFlow.Notifications.Messaging;
using EFlow.Notifications.Templates;
using Hangfire;
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
builder.Services.AddBookingClient(builder.Configuration);
builder.Services.AddNotificationServices(builder.Configuration);
builder.Services.AddNotificationsTemplates();
builder.Services.AddJobScheduler(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseHangfireDashboard(
        "/hangfire", new DashboardOptions
        {
            DashboardTitle = "EFlow Hangfire Dashboard",
            Authorization = []
        });
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider
        .GetRequiredService<TopicInitializer>()
        .WaitForTopicsCreatedAsync();

app.UseBookingReminders();

await app.RunAsync();
