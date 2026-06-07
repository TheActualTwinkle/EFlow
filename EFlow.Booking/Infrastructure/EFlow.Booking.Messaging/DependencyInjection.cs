using EFlow.Common.Extensions;
using EFlow.Common.IntegrationEvents.Booking;
using EFlow.Common.IntegrationEvents.Booking.BookingRecords;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Common.Markers;
using EFlow.Common.Messaging;
using EFlow.Common.Messaging.Settings;
using EFlow.Common.OutboxProcessing.Outbox;
using EFlow.Common.OutboxProcessing.Outbox.Interfaces;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Factories;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Factories.Interfaces;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Interfaces;
using EFlow.Common.OutboxProcessing.TopicResolving;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Booking.Messaging;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMessaging(IConfiguration configuration) =>
            services.AddKafka(configuration);

        public IServiceCollection AddOutbox(IConfiguration configuration)
        {
            services.AddOutboxProcessor(configuration);

            services.AddOutboxMessageProcessor<IKafkaMessage, KafkaMessageProcessor>();

            services.AddScoped<IOutboxMessageProcessorFactory, OutboxMessageProcessorFactory>();

            services.AddTopicResolving();

            return services;
        }
        
        private IServiceCollection AddOutboxProcessor(IConfiguration configuration)
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

        private IServiceCollection AddOutboxMessageProcessor<TMessage, TMessageProcessor>()
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
        
        private IServiceCollection AddTopicResolving() =>
            services.AddScoped<ITopicNameResolver>(_ =>
            {
                var resolver = new TopicNameResolver();

                resolver.AddMapping(typeof(SubmissionSlotCreatedIntegrationEvent).AssemblyQualifiedName!, KafkaTopics.SubmissionSlotCreatedTopic);
                resolver.AddMapping(typeof(SubmissionSlotUpdatedIntegrationEvent).AssemblyQualifiedName!, KafkaTopics.SubmissionSlotUpdatedTopic);
                resolver.AddMapping(typeof(SubmissionSlotDeletedIntegrationEvent).AssemblyQualifiedName!, KafkaTopics.SubmissionSlotDeletedTopic);
                resolver.AddMapping(typeof(BookingCreatedIntegrationEvent).AssemblyQualifiedName!, KafkaTopics.BookingCreatedTopic);
                resolver.AddMapping(typeof(BookingCancelledIntegrationEvent).AssemblyQualifiedName!, KafkaTopics.BookingCancelledTopic);

                return resolver;
            });
    }

    extension(WebApplication app)
    {
        public IApplicationBuilder UseOutbox()
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
    }
}