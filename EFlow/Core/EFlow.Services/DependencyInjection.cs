using EFlow.Services.Outbox;
using EFlow.Services.Outbox.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        var batchSize = configuration
            .GetRequiredSection("OutboxProcessorSettings")
            .GetValue<int>("BatchSize");

        if (batchSize <= 0)
            throw new InvalidOperationException("Batch size must be greater than zero.");

        var processIntervalCron = configuration
                                      .GetRequiredSection("OutboxProcessorSettings")
                                      .GetValue<string>("ProcessIntervalCron") ??
                                  throw new InvalidOperationException("Process interval cron expression is not configured.");

        var deleteIntervalCron = configuration
                                     .GetRequiredSection("OutboxProcessorSettings")
                                     .GetValue<string>("DeleteProcessedIntervalCron") ??
                                 throw new InvalidOperationException("Delete interval cron expression is not configured.");

        var deleteAfter = configuration
            .GetRequiredSection("OutboxProcessorSettings")
            .GetValue<TimeSpan>("DeleteAfter");

        services.AddSingleton<OutboxProcessorSettings>(_ => new OutboxProcessorSettings
        {
            BatchSize = batchSize,
            ProcessIntervalCron = processIntervalCron,
            DeleteIntervalCron = deleteIntervalCron,
            DeleteAfter = deleteAfter
        });

        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        return services;
    }

    public static IServiceCollection AddJobScheduler(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConnectionString = configuration.GetConnectionString("HangfireDb") ??
                                       throw new InvalidOperationException("Hangfire connection string is not configured.");

        services.AddHangfire(c =>
            c.UseDynamicJobs()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(hangfireConnectionString))
                .UseFilter(new AutomaticRetryAttribute { Attempts = 3 }));

        services.AddHangfireServer(o =>
        {
            o.Queues = ["eflow-outbox"];
            o.SchedulePollingInterval = TimeSpan.FromSeconds(10);
        });

        return services;
    }

    public static IApplicationBuilder UseOutbox(this WebApplication app)
    {
        var settings = app.Services.GetRequiredService<OutboxProcessorSettings>();

        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdateDynamic<IOutboxProcessor>(
            "ProcessOutboxMessages",
            p => p.ProcessPendingAsync(settings.BatchSize, CancellationToken.None),
            settings.ProcessIntervalCron,
#pragma warning disable CS0618 // Type or member is obsolete
            new DynamicRecurringJobOptions { QueueName = "eflow-outbox" });
#pragma warning restore CS0618 // Type or member is obsolete

        recurringJobManager.AddOrUpdateDynamic<IOutboxProcessor>(
            "DeleteOutboxMessages",
            p => p.DeleteProcessedAsync(settings.DeleteAfter, CancellationToken.None),
            settings.DeleteIntervalCron,
#pragma warning disable CS0618 // Type or member is obsolete
            new DynamicRecurringJobOptions { QueueName = "eflow-outbox" });
#pragma warning restore CS0618 // Type or member is obsolete

        return app;
    }
}