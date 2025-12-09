using EFlow.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace EFlow.WebApi.Extensions;

public static class StartupExtensions
{
    public static async Task ApplyDbMigrations(this WebApplication webApplication)
    {
        try
        {
            using var scope = webApplication.Services.CreateScope();

            var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await databaseContext.Database.MigrateAsync();
        }
        catch (Exception e)
        {
            Log.Fatal("Error on DB migration: {message}", e.Message);

            throw;
        }
    }
}