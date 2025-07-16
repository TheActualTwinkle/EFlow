using EFlow.Contracts.Messages;
using EFlow.Notifications.Messaging.Consumers.Settings;
using EFlow.Notifications.Services.NotificationServices.Interfaces;
using MassTransit;

namespace EFlow.Notifications.Messaging.Consumers;

public class SubmissionSlotCreatedConsumer(INotificationService notificationService, ConsumerSettings settings)
    : IConsumer<SubmissionSlotCreatedMessage>
{
    public async Task Consume(ConsumeContext<SubmissionSlotCreatedMessage> context) =>
        await notificationService.SendAsync(new CancellationTokenSource(settings.DefaultCancellationTimeout).Token);
}