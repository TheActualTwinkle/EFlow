using Confluent.Kafka;
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
    IServiceProvider serviceProvider)
    : IHostedService
{
    private CancellationTokenSource _cts = null!;
    
    private IServiceScope _scope = null!;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _scope = serviceProvider.CreateScope();
        
        await SubscribeAsync<SubmissionSlotCreatedIntegrationEvent>(KafkaTopics.SubmissionSlotCreatedTopic, cancellationToken);
        await SubscribeAsync<SubmissionSlotUpdatedIntegrationEvent>(KafkaTopics.SubmissionSlotUpdatedTopic, cancellationToken);
        await SubscribeAsync<BookingCreatedIntegrationEvent>(KafkaTopics.BookingCreatedTopic, cancellationToken);
        await SubscribeAsync<BookingCancelledIntegrationEvent>(KafkaTopics.BookingCancelledTopic, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cts.CancelAsync();

        _scope.Dispose();
    }

    private async Task SubscribeAsync<T>(
        string topic,
        CancellationToken cancellationToken = new())
        where T : class, IKafkaMessage
    {
        var consumer = _scope.ServiceProvider
            .GetRequiredService<ICommitLogConsumerFactory>()
            .Create<Guid, T>(GetKafkaConsumerSettings());

        await consumer.StartAsync(
            topic,
            async (message, ct) =>
            {
                await _scope.ServiceProvider
                    .GetRequiredService<IIntegrationEventProcessor<T>>()
                    .ProcessAsync(message, ct);

                return true;
            },
            cancellationToken);
    }

    private static KafkaConsumerSettings GetKafkaConsumerSettings() =>
        new()
        {
            GroupId = "notifications-service",
            AutoOffsetReset = AutoOffsetReset.Latest
        };
}