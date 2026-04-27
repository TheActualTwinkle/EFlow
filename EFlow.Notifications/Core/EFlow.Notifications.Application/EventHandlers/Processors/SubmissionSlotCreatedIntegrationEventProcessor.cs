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
    Logger<BookingCancelledIntegrationEventProcessor> logger)
    : IIntegrationEventProcessor<SubmissionSlotCreatedIntegrationEvent>
{
    public async Task ProcessAsync(SubmissionSlotCreatedIntegrationEvent @event, CancellationToken cancellationToken = new())
    {
        var (subject, body) = await templateService.CreateSubmissionSlotCreatedAsync(@event.SubmissionSlot, cancellationToken);

        foreach (var recipient in @event.NotificationRecipients)
        {
            if (recipient.Email is null)
            {
                logger.LogWarning("Recipient {UserId} does not have an email address. Skipping notification.", recipient.UserId);

                continue;
            }

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
}