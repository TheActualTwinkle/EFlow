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
    private readonly List<IServiceScope> _scopes = [];
    
    private CancellationTokenSource _cts = null!;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        await SubscribeAsync<SubmissionSlotCreatedIntegrationEvent>(KafkaTopics.SubmissionSlotCreatedTopic, _cts.Token);
        await SubscribeAsync<SubmissionSlotUpdatedIntegrationEvent>(KafkaTopics.SubmissionSlotUpdatedTopic, _cts.Token);
        await SubscribeAsync<BookingCreatedIntegrationEvent>(KafkaTopics.BookingCreatedTopic, _cts.Token);
        await SubscribeAsync<BookingCancelledIntegrationEvent>(KafkaTopics.BookingCancelledTopic, _cts.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cts.CancelAsync();

        foreach (var serviceScope in _scopes)
            serviceScope.Dispose();
    }

    private async Task SubscribeAsync<T>(
        string topic,
        CancellationToken cancellationToken = new())
        where T : class, IKafkaMessage
    {
        var scope = serviceProvider.CreateScope();
        
        _scopes.Add(scope);
        
        var consumer = scope.ServiceProvider
            .GetRequiredService<ICommitLogConsumerFactory>()
            .Create<Guid, T>(GetKafkaConsumerSettings());

        await consumer.StartAsync(
            topic,
            async (message, ct) =>
            {
                await scope.ServiceProvider
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