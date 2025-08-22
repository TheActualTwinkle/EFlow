using Confluent.Kafka;
using EFlow.Common.Messaging.Init;
using EFlow.Common.Messaging.Producers;
using EFlow.Common.Messaging.Serialization;
using EFlow.Common.Messaging.Settings;
using EFlow.Messaging.Outbox;
using EFlow.Messaging.Outbox.Interfaces;
using EFlow.Messaging.Outbox.MessageProcessing.Factories;
using EFlow.Messaging.Outbox.MessageProcessing.Factories.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFlow.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<TopicInitializer>();

        services.Configure<KafkaSettings>(configuration.GetRequiredSection("KafkaSettings"));
        services.Configure<KafkaTopicsSettings>(configuration.GetRequiredSection("KafkaSettings"));

        services.AddScoped(typeof(ICommitLogProducer<,>), typeof(CommitLogProducer<,>));
        
        services.AddSingleton<ProducerConfig>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<KafkaSettings>>().Value;

            return new ProducerConfig
            {
                BootstrapServers = settings.BootstrapServers
            };
        });

        services.AddScoped(typeof(ISerializer<>), typeof(JsonSerializer<>));

        services.AddSingleton<IAdminClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<KafkaSettings>>().Value;

            return new AdminClientBuilder(new AdminClientConfig { BootstrapServers = settings.BootstrapServers })
                .Build();
        });

        return services;
    }

    public static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        AddOutboxProcessor(services, configuration);

        services.AddScoped<IOutboxMessageProcessorFactory, OutboxMessageProcessorFactory>();

        return services;
    }

    public static IApplicationBuilder UseMessaging(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        scope.ServiceProvider
            .GetRequiredService<TopicInitializer>()
            .EnsureTopicsCreatedAsync()
            .GetAwaiter().GetResult();
        
        return app;
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

    private static void AddOutboxProcessor(IServiceCollection services, IConfiguration configuration)
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
    }
}