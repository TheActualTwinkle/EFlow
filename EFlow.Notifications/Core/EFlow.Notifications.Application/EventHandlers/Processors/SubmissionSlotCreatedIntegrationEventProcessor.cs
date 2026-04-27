using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.Email.Models;
using EFlow.Notifications.Application.EventHandlers.Processors.Interfaces;
using EFlow.Notifications.Templates.Notifications.Interfaces;
using Microsoft.Extensions.Logging;

namespace EFlow.Notifications.Application.EventHandlers.Processors;

public sealed class SubmissionSlotCreatedIntegrationEventProcessor(
    IBookingNotificationTemplateService templateService,
    IEmailNotificationService notificationService,
    Logger<SubmissionSlotCreatedIntegrationEventProcessor> logger)
    : IIntegrationEventProcessor<SubmissionSlotCreatedIntegrationEvent>
{
    public async Task ProcessAsync(SubmissionSlotCreatedIntegrationEvent @event, CancellationToken cancellationToken = new())
    {
        var (subject, body) = await templateService.CreateSubmissionSlotCreatedAsync(@event.SubmissionSlot, cancellationToken);

        foreach (var recipient in @event.NotificationRecipients)
            await notificationService.SendAsync(
                new NotificationMessage
                {
                    Subject = subject,
                    Body = body,
                    RecipientEmail = recipient.Email
                },
                cancellationToken);
    }
}