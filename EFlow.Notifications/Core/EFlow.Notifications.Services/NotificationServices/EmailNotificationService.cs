using System.Text.Json;
using Confluent.Kafka;
using EFlow.Common.Extensions;
using EFlow.Common.Messaging.Factories;
using EFlow.Common.Messaging.Settings;
using EFlow.Common.Models.SubmissionSlot;
using EFlow.Notifications.Services.NotificationServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EFlow.Notifications.Services.NotificationServices;

public class EmailNotificationService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<EmailNotificationService> logger)
    : INotificationService, IHostedService
{
    private IDisposable? _subscription;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        
        var consumerFactory = scope.ServiceProvider.GetRequiredService<ICommitLogConsumerFactory>();

        var consumer = consumerFactory.Create<Guid, SubmissionSlotCreatedMessage>(
            new KafkaConsumerSettings
            {
                GroupId = "email-notification-service",
                AutoOffsetReset = AutoOffsetReset.Latest
            });

        _subscription = consumer
            .FromTopic(KafkaConstants.SubmissionSlotCreatedTopic)
            .DoAsync(async m => await SendAsync(JsonSerializer.Serialize(m), cancellationToken))
            .Subscribe();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscription?.Dispose();

        return Task.CompletedTask;
    }

    public Task SendAsync(string message, CancellationToken cancellationToken = new())
    {
        logger.LogInformation("Testing Email Notification Service. Message: {Message}", message);

        return Task.CompletedTask;
    }
}