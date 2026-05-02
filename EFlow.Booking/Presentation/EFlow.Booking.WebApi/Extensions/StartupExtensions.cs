using System.Net.Sockets;
using EFlow.Booking.Application.Services.AdminInitialing.Interfaces;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;

namespace EFlow.Booking.WebApi.Extensions;

public static class StartupExtensions
{
    extension(WebApplication webApplication)
    {
        public async Task ApplyDbMigrationsAsync()
        {
            const int maxAttempts = 10;
            var delay = TimeSpan.FromSeconds(2);

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
                try
                {
                    using var scope = webApplication.Services.CreateScope();

                    var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    await databaseContext.Database.MigrateAsync();

                    Log.Information("Database migrations applied successfully");

                    return;
                }
                catch (Exception e) when (
                    e is NpgsqlException or SocketException ||
                    e.InnerException is NpgsqlException or SocketException)
                {
                    if (attempt == maxAttempts)
                    {
                        Log.Error(e, "Error on DB migration after {Attempts} attempts", maxAttempts);

                        throw;
                    }

                    Log.Warning(
                        e,
                        "Transient DB migration error on attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds}s",
                        attempt,
                        maxAttempts,
                        delay.TotalSeconds);

                    await Task.Delay(delay);
                }
                catch (Exception e)
                {
                    Log.Fatal(e, "Error on DB migration: {Message}", e.Message);

                    throw;
                }
        }

        public async Task InitAdminsAsync()
        {
            await using var scope = webApplication.Services.CreateAsyncScope();

            var adminInitializer = scope.ServiceProvider.GetRequiredService<IAdminInitializer>();

            await adminInitializer.InitializeAsync();
        }
    }
    
    extension(IHostEnvironment hostEnvironment)
    {
        public bool IsOpenApiGenerator() =>
            hostEnvironment.IsEnvironment("OpenApiGenerator");
    }
}