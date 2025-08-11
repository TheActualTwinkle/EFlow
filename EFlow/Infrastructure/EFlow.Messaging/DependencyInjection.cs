using EFlow.Messaging.Outbox;
using EFlow.Messaging.Outbox.Interfaces;
using Hangfire;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((_, cfg) =>
            {
                var configurationSection = configuration.GetRequiredSection("RabbitMqSettings");

                var host = configurationSection["Host"] ??
                           throw new InvalidOperationException("RabbitMqSettings:Host is not configured.");

                var port = ushort.Parse(
                    configurationSection["Port"] ??
                    throw new InvalidOperationException("RabbitMqSettings:Port is not configured."));

                var username = configurationSection["Username"] ??
                               throw new InvalidOperationException("RabbitMqSettings:Username is not configured.");

                var password = configurationSection["Password"] ??
                               throw new InvalidOperationException("RabbitMqSettings:Password is not configured.");

                cfg.Host(
                    host, port, "/", h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });
            });
        });

        return services;
    }
    
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