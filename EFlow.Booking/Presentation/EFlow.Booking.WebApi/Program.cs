using EFlow.Booking.Application;
using EFlow.Booking.Messaging;
using EFlow.Booking.Persistence;
using EFlow.Booking.WebApi;
using EFlow.Booking.WebApi.Extensions;
using EFlow.Booking.WebApi.Middleware;
using EFlow.Common.Messaging.Init;
using FluentPatcher;
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

if (builder.Environment.IsOpenApiGenerator())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();
    
    builder.Services
        .AddControllers()
        .AddJsonOptions(options => 
            options.JsonSerializerOptions.Converters.Add(new PatchableJsonConverterFactory()));

    var openApiApp = builder.Build();

    openApiApp.MapOpenApi();
    openApiApp.MapControllers();

    await openApiApp.RunAsync();

    return;
}

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

builder.Services.ConfigureIdentity(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddHealthChecks()
    .AddHangfire(o => { o.MaximumJobsFailed = 1; }, "hangfire");

builder.Services
    .AddControllers()
    .AddJsonOptions(options => 
        options.JsonSerializerOptions.Converters.Add(new PatchableJsonConverterFactory()));

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddJobScheduler(builder.Configuration);
builder.Services.AddOutbox(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);

var app = builder.Build();

await app.ApplyDbMigrationsAsync();

app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();

    app.UseHangfireDashboard(
        "/hangfire", new DashboardOptions
        {
            DashboardTitle = "EFlow Hangfire Dashboard",
            Authorization = []
        });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.MapHealthChecks(
    "/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseRouting();

await app.CreateRolesAsync();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider
    .GetRequiredService<TopicInitializer>()
    .CreateMissingTopicsAsync();

app.UseOutbox();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.InitAdminsAsync();

await app.RunAsync();
