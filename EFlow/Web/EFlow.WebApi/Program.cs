using EFlow.Application;
using EFlow.Persistence;
using EFLow.Presentation;
using EFlow.Presentation.Middleware;
using EFlow.Services;
using EFlow.WebApi;
using EFlow.WebApi.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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
        policyBuilder =>
        {
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.ConfigureIdentity(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    var assembly = typeof(AssemblyMarker).Assembly;

    var xmlPath = Path.Combine(
        Path.GetDirectoryName(assembly.Location)!,
        $"{assembly.GetName().Name}.xml");

    c.IncludeXmlComments(xmlPath);

    c.SupportNonNullableReferenceTypes();

    c.MapType<TimeSpan>(() => new OpenApiSchema
    {
        Type = "string",
        Example = new OpenApiString("00:00:00")
    });

    c.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Example = new OpenApiString("1999-03-25")
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(AssemblyMarker).Assembly);

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddOutbox(builder.Configuration);

var app = builder.Build();

await app.ApplyDbMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.MapHealthChecks(
    "/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseRouting();

await app.CreateRolesAsync();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();