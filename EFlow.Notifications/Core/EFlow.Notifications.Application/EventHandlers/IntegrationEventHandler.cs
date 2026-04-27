using Confluent.Kafka;
using EFlow.Common.Extensions;
using EFlow.Common.IntegrationEvents.Booking.BookingRecords;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Common.Markers;
using EFlow.Common.Messaging.Factories;
using EFlow.Common.Messaging.Settings;
using EFlow.Notifications.Application.EventHandlers.Processors.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EFlow.Notifications.Application.EventHandlers;

public sealed class IntegrationEventHandler(
    ICommitLogConsumerFactory consumerFactory,
    IServiceProvider serviceProvider)
    : IHostedService
{
    private readonly List<IDisposable> _subscriptions = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscriptions.Add(Subscribe<SubmissionSlotCreatedIntegrationEvent>(KafkaTopics.SubmissionSlotCreatedTopic, cancellationToken));
        _subscriptions.Add(Subscribe<SubmissionSlotUpdatedIntegrationEvent>(KafkaTopics.SubmissionSlotUpdatedTopic, cancellationToken));
        _subscriptions.Add(Subscribe<BookingCreatedIntegrationEvent>(KafkaTopics.BookingCreatedTopic, cancellationToken));
        _subscriptions.Add(Subscribe<BookingCancelledIntegrationEvent>(KafkaTopics.BookingCancelledTopic, cancellationToken));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscriptions.ForEach(subscription => subscription.Dispose());
        _subscriptions.Clear();
        
        return Task.CompletedTask;
    }

    private static KafkaConsumerSettings GetKafkaConsumerSettings() =>
        new()
        {
            GroupId = "notifications-service",
            AutoOffsetReset = AutoOffsetReset.Latest
        };

    private IDisposable Subscribe<T>(
        string topic,
        CancellationToken cancellationToken = new())
        where T : class, IKafkaMessage =>
        consumerFactory
            .Create<Guid, T>(GetKafkaConsumerSettings())
            .FromTopic(topic)
            .DoAsync(message => serviceProvider
                .GetRequiredService<IIntegrationEventProcessor<T>>()
                .ProcessAsync(message, cancellationToken))
            .Subscribe();
}