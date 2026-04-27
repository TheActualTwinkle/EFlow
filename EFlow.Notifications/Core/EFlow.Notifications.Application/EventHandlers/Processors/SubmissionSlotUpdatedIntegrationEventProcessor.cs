using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.Email.Models;
using EFlow.Notifications.Application.EventHandlers.Processors.Interfaces;
using EFlow.Notifications.Templates.Notifications.Interfaces;
using Microsoft.Extensions.Logging;

namespace EFlow.Notifications.Application.EventHandlers.Processors;

public sealed class SubmissionSlotUpdatedIntegrationEventProcessor(
    IBookingNotificationTemplateService templateService,
    IEmailNotificationService notificationService,
    Logger<SubmissionSlotUpdatedIntegrationEventProcessor> logger)
    : IIntegrationEventProcessor<SubmissionSlotUpdatedIntegrationEvent>
{
    public async Task ProcessAsync(SubmissionSlotUpdatedIntegrationEvent @event, CancellationToken cancellationToken = new())
    {
        var (subject, body) = await templateService.CreateSubmissionSlotUpdatedAsync(@event.OldSubmissionSlot, @event.NewSubmissionSlot, cancellationToken);

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