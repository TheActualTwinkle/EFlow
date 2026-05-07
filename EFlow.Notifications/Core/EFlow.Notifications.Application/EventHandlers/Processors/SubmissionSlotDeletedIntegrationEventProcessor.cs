using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.Email.Models;
using EFlow.Notifications.Application.EventHandlers.Processors.Interfaces;
using EFlow.Notifications.Templates.Notifications.Interfaces;

namespace EFlow.Notifications.Application.EventHandlers.Processors;

public sealed class SubmissionSlotDeletedIntegrationEventProcessor(
    IBookingNotificationTemplateService templateService,
    IEmailNotificationService notificationService)
    : IIntegrationEventProcessor<SubmissionSlotDeletedIntegrationEvent>
{
    public async Task ProcessAsync(SubmissionSlotDeletedIntegrationEvent @event, CancellationToken cancellationToken = new())
    {
        var (subject, body) = await templateService.CreateSubmissionSlotDeletedAsync(@event.SubmissionSlot, cancellationToken);

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
