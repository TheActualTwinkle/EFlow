using Confluent.Kafka;
using EFlow.Common.Extensions;
using EFlow.Common.IntegrationEvents.Booking;
using EFlow.Common.IntegrationEvents.Booking.BookingRecords;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Common.OutboxProcessing.Outbox;
using EFlow.Common.OutboxProcessing.Outbox.Interfaces;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Factories;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Factories.Interfaces;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Interfaces;
using EFlow.Common.Markers;
using EFlow.Common.Messaging.Init;
using EFlow.Common.Messaging.Producers;
using EFlow.Common.Messaging.Serialization;
using EFlow.Common.Messaging.Settings;
using EFlow.Common.OutboxProcessing.TopicResolving;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFlow.Booking.Messaging;

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
                BootstrapServers = settings.BootstrapServers,
                AllowAutoCreateTopics = false,
                ReconnectBackoffMs = 1000,
                MessageSendMaxRetries = settings.Retries,
                MessageTimeoutMs = settings.MessageTimeoutMs
            };
        });

        services.AddScoped(typeof(ISerializer<>), typeof(DefaultSerializer<>));

        services.AddSingleton<IAdminClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<KafkaSettings>>().Value;

            return new AdminClientBuilder(new AdminClientConfig { BootstrapServers = settings.BootstrapServers })
                .Build();
        });

        AddTopicResolving(services);

        return services;
    }

    public static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOutboxProcessor(configuration);

        services.AddOutboxMessageProcessor<IKafkaMessage, KafkaMessageProcessor>();

        services.AddScoped<IOutboxMessageProcessorFactory, OutboxMessageProcessorFactory>();

        return services;
    }

    public static IApplicationBuilder UseOutbox(this WebApplication app)
    {
        var settings = app.Services.GetRequiredService<OutboxProcessorSettings>();

        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdateDynamic<IOutboxProcessor>(
            "ProcessOutboxMessages",
            p => p.ProcessPendingAsync(settings.BatchSize, new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token),
            settings.ProcessInterval.ToCronExpression(),
#pragma warning disable CS0618 // Type or member is obsolete
            new DynamicRecurringJobOptions { QueueName = "eflow-outbox" });
#pragma warning restore CS0618 // Type or member is obsolete

        recurringJobManager.AddOrUpdateDynamic<IOutboxProcessor>(
            "DeleteOutboxMessages",
            p => p.DeleteProcessedAsync(settings.DeleteAfter, new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token),
            settings.DeleteInterval.ToCronExpression(),
#pragma warning disable CS0618 // Type or member is obsolete
            new DynamicRecurringJobOptions { QueueName = "eflow-outbox" });
#pragma warning restore CS0618 // Type or member is obsolete

        return app;
    }

    private static IServiceCollection AddOutboxProcessor(this IServiceCollection services, IConfiguration configuration)
    {
        var batchSize = configuration
            .GetRequiredSection("OutboxProcessorSettings")
            .GetValue<int>("BatchSize");

        if (batchSize <= 0)
            throw new InvalidOperationException("Batch size must be greater than zero.");

        var processInterval = configuration
            .GetRequiredSection("OutboxProcessorSettings")
            .GetValue<TimeSpan>("ProcessInterval");

        var deleteInterval = configuration
            .GetRequiredSection("OutboxProcessorSettings")
            .GetValue<TimeSpan>("DeleteProcessedInterval");

        var deleteAfter = configuration
            .GetRequiredSection("OutboxProcessorSettings")
            .GetValue<TimeSpan>("DeleteAfter");

        services.AddSingleton<OutboxProcessorSettings>(_ => new OutboxProcessorSettings
        {
            BatchSize = batchSize,
            ProcessInterval = processInterval,
            DeleteInterval = deleteInterval,
            DeleteAfter = deleteAfter
        });

        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        return services;
    }

    private static IServiceCollection AddOutboxMessageProcessor<TMessage, TMessageProcessor>(this IServiceCollection services)
        where TMessageProcessor : class, IOutboxMessageProcessor
    {
        var messageType = typeof(TMessage);

        var assemblies = new[]
        {
            typeof(IntegrationEventsAssemblyMarker).Assembly
        };

        var messageTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(type => messageType.IsAssignableFrom(type) && !type.IsAbstract)
            .ToList();

        foreach (var type in messageTypes)
            services.AddKeyedScoped<IOutboxMessageProcessor, TMessageProcessor>(type);

        return services;
    }

    private static void AddTopicResolving(IServiceCollection services) =>
        services.AddScoped<ITopicNameResolver>(_ =>
        {
            var resolver = new TopicNameResolver();

            resolver.AddMapping(typeof(SubmissionSlotCreatedIntegrationEvent).AssemblyQualifiedName!, KafkaTopics.SubmissionSlotCreatedTopic);
            resolver.AddMapping(typeof(BookingCreatedIntegrationEvent).AssemblyQualifiedName!, KafkaTopics.BookingCreatedTopic);
            resolver.AddMapping(typeof(BookingCancelledIntegrationEvent).AssemblyQualifiedName!, KafkaTopics.BookingCancelledTopic);
            resolver.AddMapping(typeof(SubmissionSlotUpdatedIntegrationEvent).AssemblyQualifiedName!, KafkaTopics.SubmissionSlotUpdatedTopic);

            return resolver;
        });
}
