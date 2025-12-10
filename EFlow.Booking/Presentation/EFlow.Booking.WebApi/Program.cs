using EFlow.Booking.Application;
using EFlow.Booking.Messaging;
using EFlow.Booking.Persistence;
using EFlow.Booking.WebApi;
using EFlow.Booking.WebApi.Extensions;
using EFlow.Booking.WebApi.Middleware;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((_, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration));

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policyBuilder => policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.ConfigureIdentity(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddHealthChecks()
    .AddHangfire(o => { o.MaximumJobsFailed = 1; }, "hangfire");

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddJobScheduler(builder.Configuration);
builder.Services.AddOutbox(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);

var app = builder.Build();

await app.ApplyDbMigrations();

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

app.UseCors("AllowAll");

app.MapHealthChecks(
    "/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseRouting();

await app.CreateRolesAsync();

await app.UseMessagingAsync();
app.UseOutbox();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();